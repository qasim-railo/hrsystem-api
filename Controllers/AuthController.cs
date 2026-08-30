using HRSystem.API.Models.Auth;
using HRSystem.API.DTOs;
using HRSystem.API.Data;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using HRSystem.API.Tenancy;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly AuthService _auth;
        private readonly IAuditService _audit;
        private readonly CurrentTenant _tenant;

        public AuthController(IConfiguration config, AppDbContext db, AuthService auth, IAuditService audit, CurrentTenant tenant)
        {
            _config = config;
            _db = db;
            _auth = auth;
            _audit = audit;
            _tenant = tenant;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (string.IsNullOrWhiteSpace(login.Email))
                return BadRequest("Email is required.");

            var user = await _auth.FindUserByEmailAsync(_db, login.Email);
            if (user == null || !_auth.VerifyPassword(login.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            _tenant.SetTenant(user.TenantId);
            await _audit.LogAsync("Login", "User", user.Id.ToString(), user.Username, "{\"result\":\"success\"}");
            return Ok(new { token = GenerateToken(user) });
        }

        private string GenerateToken(AppUser user)
        {
            var jwtSettings = _config.GetSection("Jwt").Get<JwtSettings>();
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new("tenant_id", user.TenantId.ToString())
                ,new("user_id", user.Id.ToString())
            };
            claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));
            claims.AddRange(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new Claim("permission", rp.Permission.Name)).DistinctBy(c => c.Value));
            claims.AddRange(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new Claim("permission_scope", $"{rp.Permission.Name}:{rp.DataScope}:{rp.ScopeIdsJson}")));
            var employeeId = _db.Employees.Where(e => e.Email == user.Username).Select(e => (int?)e.EmployeeId).FirstOrDefault();
            if (employeeId.HasValue) claims.Add(new Claim("employee_id", employeeId.Value.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
