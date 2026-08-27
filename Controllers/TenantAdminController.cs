using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Authorize(Policy = "Users.Manage")]
[Route("api/tenant")]
public class TenantAdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public TenantAdminController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        return Ok(new { tenant.TenantId, tenant.Name, tenant.Code, tenant.Country, tenant.Currency, tenant.TimeZone, tenant.Status, tenant.LifecycleStatus });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(TenantProfileDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Name)) tenant.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Country)) tenant.Country = dto.Country.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Currency)) tenant.Currency = dto.Currency.Trim();
        if (!string.IsNullOrWhiteSpace(dto.TimeZone)) tenant.TimeZone = dto.TimeZone.Trim();
        await _db.SaveChangesAsync();
        return Ok(new { tenant.TenantId, tenant.Name, tenant.Code, tenant.Country, tenant.Currency, tenant.TimeZone });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        return Ok(await _db.TenantSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync());
    }

    [HttpPut("settings/{key}")]
    public async Task<IActionResult> SetSetting(string key, TenantSettingDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        if (string.IsNullOrWhiteSpace(key)) return BadRequest("Setting key is required.");
        var setting = await _db.TenantSettings.SingleOrDefaultAsync(x => x.TenantId == id && x.Key == key);
        if (setting == null) _db.TenantSettings.Add(setting = new TenantSetting { TenantId = id, Key = key.Trim() });
        setting.Value = dto.Value ?? string.Empty;
        await _db.SaveChangesAsync();
        return Ok(setting);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        var subscription = await _db.Subscriptions.AsNoTracking().Where(s => s.TenantId == id)
            .Select(s => new { s.SubscriptionId, s.PlanId, Plan = s.Plan.Name, s.Status, s.RenewalDate }).SingleOrDefaultAsync();
        return Ok(new {
            tenant = new { tenant.TenantId, tenant.Name, tenant.Code },
            users = await _db.Users.CountAsync(u => u.TenantId == id),
            activeUsers = await _db.Users.CountAsync(u => u.TenantId == id && u.IsActive),
            roles = await _db.Roles.CountAsync(r => r.TenantId == id && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")),
            departments = await _db.Department.CountAsync(),
            branches = await _db.Companies.CountAsync(),
            employees = await _db.Employees.CountAsync(),
            usage = new { tenant.StorageUsedBytes },
            subscription
        });
    }
}
