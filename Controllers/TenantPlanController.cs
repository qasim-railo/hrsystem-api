using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRSystem.API.Tenancy;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/tenant/plan")]
[Authorize]
public class TenantPlanController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _currentTenant;
    public TenantPlanController(AppDbContext db, ICurrentTenant currentTenant)
    {
        _db = db;
        _currentTenant = currentTenant;
    }

    [HttpGet]
    public async Task<ActionResult<PlanDto>> GetCurrentPlan()
    {
        if (_currentTenant.TenantId is not int tenantId) return Forbid();
        var plan = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.Plan)
            .ThenInclude(p => p!.Features)
            .Select(t => t.Plan)
            .Where(p => p != null)
            .Select(p => new PlanDto
            {
                PlanId = p!.PlanId, Code = p.Code, Name = p.Name,
                MaxEmployees = p.MaxEmployees, MaxUsers = p.MaxUsers,
                MaxBranches = p.MaxBranches, MaxStorageBytes = p.MaxStorageBytes,
                FeatureCodes = p.Features.Where(f => f.IsEnabled).Select(f => f.FeatureCode).ToList()
            })
            .SingleOrDefaultAsync();
        return plan == null ? NotFound("The current tenant has no plan assigned.") : Ok(plan);
    }
}
