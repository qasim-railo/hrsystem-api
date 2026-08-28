using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public AuditController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> Get([FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        if (_tenant.TenantId is not int) return Forbid();
        limit = Math.Clamp(limit, 1, 500);
        var logs = await _db.AuditLogs.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(limit)
            .Select(x => new AuditLogDto(x.AuditLogId, x.Action, x.Entity, x.EntityId, x.UserId, x.CreatedAt, x.Details))
            .ToListAsync(cancellationToken);
        return Ok(logs);
    }
}
