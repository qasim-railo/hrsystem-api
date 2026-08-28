using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Models.Auth;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/recycle-bin")]
public sealed class RecycleBinController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public RecycleBinController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecycleBinItemDto>>> Get(CancellationToken cancellationToken)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        var result = new List<RecycleBinItemDto>();
        result.AddRange(await _db.Employees.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.ArchivedAt != null)
            .Select(x => new RecycleBinItemDto("Employee", x.EmployeeId.ToString(), x.FirstName + " " + x.LastName, x.ArchivedAt!.Value)).ToListAsync(cancellationToken));
        result.AddRange(await _db.Department.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.ArchivedAt != null)
            .Select(x => new RecycleBinItemDto("Department", x.DepartmentId.ToString(), x.Name, x.ArchivedAt!.Value)).ToListAsync(cancellationToken));
        result.AddRange(await _db.Assets.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.ArchivedAt != null)
            .Select(x => new RecycleBinItemDto("Asset", x.Id.ToString(), x.Name, x.ArchivedAt!.Value)).ToListAsync(cancellationToken));
        result.AddRange(await _db.Users.IgnoreQueryFilters().Where(x => x.TenantId == tenantId && x.ArchivedAt != null)
            .Select(x => new RecycleBinItemDto("User", x.Id.ToString(), x.Username, x.ArchivedAt!.Value)).ToListAsync(cancellationToken));
        return Ok(result.OrderByDescending(x => x.ArchivedAt));
    }

    [HttpPost("{entityType}/{id:int}/restore")]
    public async Task<IActionResult> Restore(string entityType, int id, CancellationToken cancellationToken)
        => await SetArchiveState(entityType, id, null, false, cancellationToken);

    [HttpDelete("{entityType}/{id:int}/purge")]
    public async Task<IActionResult> Purge(string entityType, int id, CancellationToken cancellationToken)
        => await SetArchiveState(entityType, id, DateTime.UtcNow, true, cancellationToken);

    [HttpPost("{entityType}/{id:int}")]
    public async Task<IActionResult> Archive(string entityType, int id, CancellationToken cancellationToken)
        => await SetArchiveState(entityType, id, DateTime.UtcNow, false, cancellationToken);

    private async Task<IActionResult> SetArchiveState(string entityType, int id, DateTime? archivedAt, bool purge, CancellationToken cancellationToken)
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        object? entity = entityType.ToLowerInvariant() switch
        {
            "employee" => await _db.Employees.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.EmployeeId == id, cancellationToken),
            "department" => await _db.Department.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.DepartmentId == id, cancellationToken),
            "asset" => await _db.Assets.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken),
            "user" => await _db.Users.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken),
            _ => null
        };
        if (entity is null) return NotFound();
        if (purge)
        {
            if (entity is Employee) return BadRequest("Employees must be restored or archived, not permanently purged.");
            _db.Remove(entity);
        }
        else
        {
            switch (entity)
            {
                case Employee employee: employee.ArchivedAt = archivedAt; break;
                case Department department: department.ArchivedAt = archivedAt; break;
                case Asset asset: asset.ArchivedAt = archivedAt; break;
                case AppUser user: user.ArchivedAt = archivedAt; user.IsActive = archivedAt == null; break;
            }
        }
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
