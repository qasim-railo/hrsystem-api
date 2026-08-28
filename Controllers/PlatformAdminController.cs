using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/platform")]
[Authorize(Policy = "Platform.Tenants")]
public class PlatformAdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlatformAdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<IEnumerable<PlatformTenantDto>>> GetTenants([FromQuery] string? search)
    {
        var tenants = _db.Tenants.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            tenants = tenants.Where(t => t.Name.Contains(search) || t.Code.Contains(search));

        var result = await tenants
            .Select(t => new PlatformTenantDto
            {
                TenantId = t.TenantId,
                Name = t.Name,
                Code = t.Code,
                Status = t.Status,
                LifecycleStatus = t.LifecycleStatus,
                PlanName = t.PlanName,
                TrialStartDate = t.TrialStartDate,
                TrialEndDate = t.TrialEndDate,
                BillingStatus = t.BillingStatus,
                StorageUsedBytes = _db.FileRecords.Where(f => f.TenantId == t.TenantId && f.IsCurrent && !f.IsDeleted && f.Status == "Active").Sum(f => (long?)f.Size) ?? 0,
                StorageLimitBytes = t.Plan.MaxStorageBytes,
                UserCount = _db.Users.Count(u => u.TenantId == t.TenantId),
                EmployeeCount = _db.Employees.IgnoreQueryFilters().Count(e => e.TenantId == t.TenantId),
                CompanyCount = _db.Companies.IgnoreQueryFilters().Count(c => c.TenantId == t.TenantId)
            })
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("tenants/{id:int}")]
    public async Task<ActionResult<PlatformTenantDto>> GetTenant(int id)
    {
        var tenant = await BuildTenantQuery().SingleOrDefaultAsync(t => t.TenantId == id);
        return tenant == null ? NotFound() : Ok(tenant);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<PlatformStatisticsDto>> GetStatistics()
    {
        return Ok(new PlatformStatisticsDto
        {
            TotalTenants = await _db.Tenants.CountAsync(),
            ActiveTenants = await _db.Tenants.CountAsync(t => t.Status == "Active"),
            TrialTenants = await _db.Tenants.CountAsync(t => t.Status == "Trial"),
            SuspendedTenants = await _db.Tenants.CountAsync(t => t.Status == "Suspended"),
            TotalUsers = await _db.Users.CountAsync(),
            TotalEmployees = await _db.Employees.IgnoreQueryFilters().CountAsync(),
            TotalStorageUsedBytes = await _db.FileRecords.Where(f => f.IsCurrent && !f.IsDeleted && f.Status == "Active").SumAsync(f => (long?)f.Size) ?? 0
        });
    }

    [HttpPost("tenants/{id:int}/activate")]
    public Task<IActionResult> Activate(int id) => ChangeStatus(id, "Active", "Activated");

    [HttpPost("tenants/{id:int}/suspend")]
    public Task<IActionResult> Suspend(int id) => ChangeStatus(id, "Suspended", "Suspended");

    [HttpPost("tenants/{id:int}/resume")]
    public Task<IActionResult> Resume(int id) => ChangeStatus(id, "Active", "Resumed");

    [HttpPost("tenants/{id:int}/archive")]
    public Task<IActionResult> Archive(int id) => ChangeStatus(id, "Archived", "Archived");

    private async Task<IActionResult> ChangeStatus(int id, string status, string action)
    {
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();

        tenant.Status = status;
        tenant.LifecycleStatus = status;
        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            TenantId = id,
            Action = action,
            Entity = nameof(Tenant),
            EntityId = id.ToString(),
            UserId = User.Identity?.Name ?? "unknown",
            Details = $"Tenant status changed to {status}."
        });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<PlatformTenantDto> BuildTenantQuery()
    {
        return _db.Tenants.AsNoTracking().Select(t => new PlatformTenantDto
        {
            TenantId = t.TenantId,
            Name = t.Name,
            Code = t.Code,
            Status = t.Status,
            LifecycleStatus = t.LifecycleStatus,
            PlanName = t.PlanName,
            TrialStartDate = t.TrialStartDate,
            TrialEndDate = t.TrialEndDate,
            BillingStatus = t.BillingStatus,
            StorageUsedBytes = _db.FileRecords.Where(f => f.TenantId == t.TenantId && f.IsCurrent && !f.IsDeleted && f.Status == "Active").Sum(f => (long?)f.Size) ?? 0,
            StorageLimitBytes = t.Plan.MaxStorageBytes,
            UserCount = _db.Users.Count(u => u.TenantId == t.TenantId),
            EmployeeCount = _db.Employees.IgnoreQueryFilters().Count(e => e.TenantId == t.TenantId),
            CompanyCount = _db.Companies.IgnoreQueryFilters().Count(c => c.TenantId == t.TenantId)
        });
    }
}
