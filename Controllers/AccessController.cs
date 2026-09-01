using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Tenancy;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text.Json;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/access")]
[Authorize(Policy = "Users.Manage")]
public class AccessController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuthService _auth;
    private readonly ICurrentTenant _tenant;

    public AccessController(AppDbContext db, AuthService auth, ICurrentTenant tenant)
    {
        _db = db;
        _auth = auth;
        _tenant = tenant;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserAccessDto>>> Users()
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        return Ok(await _db.Users.Where(u => u.TenantId == tenantId).Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserAccessDto
            {
                Id = u.Id,
                Email = u.Username,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Where(ur => !ur.Role.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")).Select(ur => ur.Role.Name).ToList(),
                Permissions = u.UserRoles.SelectMany(ur => ur.Role.RolePermissions).Where(rp => rp.Permission.Name != "Platform.Tenants").Select(rp => rp.Permission.Name).Distinct().ToList()
            }).ToListAsync());
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        var roles = await _db.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
        .Where(r => r.TenantId == tenantId && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants"))
            .ToListAsync();
        return Ok(roles.Select(r => new { r.Id, r.Name, Permissions = r.RolePermissions.Select(rp => new { name = rp.Permission.Name, dataScope = NormalizeScope(rp.DataScope), scopeIds = ParseScopeIds(rp.ScopeIdsJson) }) }));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions() => Ok(await _db.Permissions
        .Where(p => p.Name != "Platform.Tenants").OrderBy(p => p.Name).ToListAsync());

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(RoleAccessDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Role name is required.");
        if (_tenant.TenantId is not int tenantId) return Forbid();
        if (await _db.Roles.AnyAsync(r => r.TenantId == tenantId && r.Name == dto.Name && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants"))) return Conflict("Role already exists.");
        var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
        if (permissions.Count != dto.Permissions.Distinct().Count() || permissions.Any(p => p.Name == "Platform.Tenants")) return BadRequest("One or more permissions are not available to tenant administrators.");
        var role = new Role { TenantId = tenantId, Name = dto.Name.Trim(), RolePermissions = permissions.Select(p => {
            var scope = dto.PermissionScopes.FirstOrDefault(s => s.Permission == p.Name);
            return new RolePermission { PermissionId = p.Id, DataScope = NormalizeScope(scope?.DataScope), ScopeIdsJson = JsonSerializer.Serialize(scope?.ScopeIds ?? new()) };
        }).ToList() };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Created($"api/access/roles/{role.Id}", new { role.Id, role.Name, Permissions = role.RolePermissions.Select(rp => new { name = permissions.Single(p => p.Id == rp.PermissionId).Name, dataScope = rp.DataScope, scopeIds = JsonSerializer.Deserialize<List<int>>(rp.ScopeIdsJson) ?? new() }) });
    }

    [HttpPut("roles/{id}")]
    public async Task<IActionResult> UpdateRole(int id, RoleAccessDto dto)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        var role = await _db.Roles.Include(r => r.RolePermissions).SingleOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants"));
        if (role == null) return NotFound();
        var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
        if (permissions.Count != dto.Permissions.Distinct().Count() || permissions.Any(p => p.Name == "Platform.Tenants")) return BadRequest("One or more permissions are not available to tenant administrators.");
        role.Name = dto.Name.Trim();
        role.RolePermissions.Clear();
        role.RolePermissions = permissions.Select(p => {
            var scope = dto.PermissionScopes.FirstOrDefault(s => s.Permission == p.Name);
            return new RolePermission { RoleId = role.Id, PermissionId = p.Id, DataScope = NormalizeScope(scope?.DataScope), ScopeIdsJson = JsonSerializer.Serialize(scope?.ScopeIds ?? new()) };
        }).ToList();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static string NormalizeScope(string? scope) => scope is "TenantWide" or "SelectedCompanies" or "SelectedBranches" or "SelectedDepartments" or "OwnTeam" or "Self" ? scope : "TenantWide";
    private static List<int> ParseScopeIds(string? json) => string.IsNullOrWhiteSpace(json) ? new() : JsonSerializer.Deserialize<List<int>>(json) ?? new();

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Email and password are required.");
        var email = dto.Email.Trim().ToLowerInvariant();
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _)) return BadRequest("A valid email address is required.");
        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username.ToLower() == email)) return Conflict("Email already exists.");
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId && dto.Roles.Contains(r.Name) && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")).ToListAsync();
        if (roles.Count != dto.Roles.Distinct().Count()) return BadRequest("One or more roles do not exist.");
        var user = new AppUser { TenantId = tenantId, Username = email, PasswordHash = _auth.HashPassword(dto.Password), IsActive = dto.IsActive };
        user.UserRoles = roles.Select(r => new UserRole { RoleId = r.Id }).ToList();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created($"api/access/users/{user.Id}", new UserAccessDto { Id = user.Id, Email = user.Username, IsActive = user.IsActive, Roles = roles.Select(r => r.Name).ToList(), Permissions = new() });
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, CreateUserDto dto)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        var user = await _db.Users.Include(u => u.UserRoles).SingleOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (user == null) return NotFound();
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId && dto.Roles.Contains(r.Name) && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")).ToListAsync();
        if (roles.Count != dto.Roles.Distinct().Count()) return BadRequest("One or more roles do not exist.");
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password)) user.PasswordHash = _auth.HashPassword(dto.Password);
        user.UserRoles.Clear();
        user.UserRoles = roles.Select(r => new UserRole { UserId = user.Id, RoleId = r.Id }).ToList();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("users/invite")]
    public async Task<IActionResult> InviteUser(InviteUserDto dto)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required.");
        var email = dto.Email.Trim().ToLowerInvariant();
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _)) return BadRequest("A valid email address is required.");
        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username.ToLower() == email))
            return Conflict("Email already exists.");
        var roles = await _db.Roles.Where(r => r.TenantId == tenantId && dto.Roles.Contains(r.Name)
            && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")).ToListAsync();
        if (roles.Count != dto.Roles.Distinct().Count()) return BadRequest("One or more roles do not exist.");
        var temporaryPassword = string.IsNullOrWhiteSpace(dto.TemporaryPassword)
            ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(12)) : dto.TemporaryPassword;
        var user = new AppUser { TenantId = tenantId, Username = email,
            PasswordHash = _auth.HashPassword(temporaryPassword), IsActive = dto.IsActive,
            UserRoles = roles.Select(r => new UserRole { RoleId = r.Id }).ToList() };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created($"api/access/users/{user.Id}", new { user.Id, Email = user.Username, temporaryPassword });
    }

    [HttpPost("users/{id}/disable")]
    public async Task<IActionResult> DisableUser(int id)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();

        var actingUserIdClaim = User.FindFirstValue("user_id");
        if (!int.TryParse(actingUserIdClaim, out var actingUserId) || actingUserId <= 0) return Forbid();
        if (actingUserId == id) return BadRequest("You cannot deactivate your own account. Ask another administrator to do this.");

        var actingUser = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == actingUserId && u.TenantId == tenantId);
        if (actingUser == null) return Forbid();

        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId);
        if (user == null) return NotFound();

        var actingRoleNames = actingUser.UserRoles.Select(ur => ur.Role.Name);
        var targetRoleNames = user.UserRoles.Select(ur => ur.Role.Name);
        if (!RoleHierarchy.CanManage(actingRoleNames, targetRoleNames))
            return StatusCode(403, "You can only deactivate users in a lower management rank than your own.");

        user.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
