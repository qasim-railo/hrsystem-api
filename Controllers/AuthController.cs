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

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly AuthService _auth;

        public AuthController(IConfiguration config, AppDbContext db, AuthService auth)
        {
            _config = config;
            _db = db;
            _auth = auth;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _auth.FindUserAsync(_db, login.Username);
            if (user == null || !_auth.VerifyPassword(login.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            return Ok(new { token = GenerateToken(user) });
        }

        private string GenerateToken(AppUser user)
        {
            var jwtSettings = _config.GetSection("Jwt").Get<JwtSettings>();
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new("tenant_id", user.TenantId.ToString())
            };
            claims.AddRange(user.UserRoles.Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name)));
            claims.AddRange(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new Claim("permission", rp.Permission.Name)).DistinctBy(c => c.Value));
            claims.AddRange(user.UserRoles.SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => new Claim("permission_scope", $"{rp.Permission.Name}:{rp.DataScope}:{rp.ScopeIdsJson}")));

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
