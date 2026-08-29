using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Services;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/tenant/plan")]
[Authorize]
public class TenantPlanController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _currentTenant;
    private readonly ISubscriptionCheckService _subscriptionCheckService;

    public TenantPlanController(AppDbContext db, ICurrentTenant currentTenant, ISubscriptionCheckService subscriptionCheckService)
    {
        _db = db;
        _currentTenant = currentTenant;
        _subscriptionCheckService = subscriptionCheckService;
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

    [HttpGet("feature/{featureCode}")]
    public async Task<ActionResult<FeatureCheckResult>> CheckFeature(string featureCode)
    {
        if (_currentTenant.TenantId is not int tenantId)
            return Forbid();

        var result = await _subscriptionCheckService.CheckFeatureAsync(tenantId, featureCode);
        if (!result.Allowed && result.UpgradeRequired)
            return StatusCode(StatusCodes.Status403Forbidden, result);

        return Ok(result);
    }

    [HttpGet("features")]
    public async Task<ActionResult<IEnumerable<string>>> GetFeatureCodes()
    {
        if (_currentTenant.TenantId is not int tenantId)
            return Forbid();

        var featureCodes = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Include(t => t.Plan)
            .ThenInclude(p => p!.Features)
            .Select(t => t.Plan!.Features.Where(f => f.IsEnabled).Select(f => f.FeatureCode))
            .FirstOrDefaultAsync();

        return featureCodes == null ? NotFound("The current tenant has no plan assigned.") : Ok(featureCodes);
    }
}
