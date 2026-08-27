using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/access")]
[Authorize(Policy = "Users.Manage")]
public class AccessController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuthService _auth;

    public AccessController(AppDbContext db, AuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserAccessDto>>> Users()
    {
        return Ok(await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserAccessDto
            {
                Id = u.Id,
                Username = u.Username,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                Permissions = u.UserRoles.SelectMany(ur => ur.Role.RolePermissions).Select(rp => rp.Permission.Name).Distinct().ToList()
            }).ToListAsync());
    }

    [HttpGet("roles")]
    public async Task<IActionResult> Roles() => Ok(await _db.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
        .Select(r => new { r.Id, r.Name, Permissions = r.RolePermissions.Select(rp => rp.Permission.Name) }).ToListAsync());

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions() => Ok(await _db.Permissions.OrderBy(p => p.Name).ToListAsync());

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole(RoleAccessDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Role name is required.");
        if (await _db.Roles.AnyAsync(r => r.Name == dto.Name)) return Conflict("Role already exists.");
        var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
        if (permissions.Count != dto.Permissions.Distinct().Count()) return BadRequest("One or more permissions do not exist.");
        var role = new Role { Name = dto.Name.Trim(), RolePermissions = permissions.Select(p => new RolePermission { PermissionId = p.Id }).ToList() };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Created($"api/access/roles/{role.Id}", new { role.Id, role.Name, Permissions = permissions.Select(p => p.Name) });
    }

    [HttpPut("roles/{id}")]
    public async Task<IActionResult> UpdateRole(int id, RoleAccessDto dto)
    {
        var role = await _db.Roles.Include(r => r.RolePermissions).SingleOrDefaultAsync(r => r.Id == id);
        if (role == null) return NotFound();
        var permissions = await _db.Permissions.Where(p => dto.Permissions.Contains(p.Name)).ToListAsync();
        if (permissions.Count != dto.Permissions.Distinct().Count()) return BadRequest("One or more permissions do not exist.");
        role.Name = dto.Name.Trim();
        role.RolePermissions.Clear();
        role.RolePermissions = permissions.Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id }).ToList();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Username and password are required.");
        if (await _db.Users.AnyAsync(u => u.Username == dto.Username)) return Conflict("Username already exists.");
        var roles = await _db.Roles.Where(r => dto.Roles.Contains(r.Name)).ToListAsync();
        if (roles.Count != dto.Roles.Distinct().Count()) return BadRequest("One or more roles do not exist.");
        var user = new AppUser { Username = dto.Username.Trim(), PasswordHash = _auth.HashPassword(dto.Password), IsActive = dto.IsActive };
        user.UserRoles = roles.Select(r => new UserRole { RoleId = r.Id }).ToList();
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Created($"api/access/users/{user.Id}", new UserAccessDto { Username = user.Username, Roles = roles.Select(r => r.Name).ToList(), Permissions = new() });
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, CreateUserDto dto)
    {
        var user = await _db.Users.Include(u => u.UserRoles).SingleOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        var roles = await _db.Roles.Where(r => dto.Roles.Contains(r.Name)).ToListAsync();
        if (roles.Count != dto.Roles.Distinct().Count()) return BadRequest("One or more roles do not exist.");
        user.IsActive = dto.IsActive;
        if (!string.IsNullOrWhiteSpace(dto.Password)) user.PasswordHash = _auth.HashPassword(dto.Password);
        user.UserRoles.Clear();
        user.UserRoles = roles.Select(r => new UserRole { UserId = user.Id, RoleId = r.Id }).ToList();
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
