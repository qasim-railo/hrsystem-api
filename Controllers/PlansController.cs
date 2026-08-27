using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/platform/plans")]
[Authorize(Policy = "Platform.Tenants")]
public class PlansController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlansController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlans()
        => Ok(await _db.Plans.AsNoTracking().OrderBy(p => p.PlanId).Select(ToDto).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanDto>> GetPlan(int id)
    {
        var plan = await _db.Plans.AsNoTracking().Where(p => p.PlanId == id).Select(ToDto).SingleOrDefaultAsync();
        return plan == null ? NotFound() : Ok(plan);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlanDto>> UpdatePlan(int id, UpdatePlanDto dto)
    {
        if (dto.MaxEmployees < 0 || dto.MaxUsers < 0 || dto.MaxBranches < 0 || dto.MaxStorageBytes < 0)
            return BadRequest("Plan limits cannot be negative.");

        var plan = await _db.Plans.Include(p => p.Features).SingleOrDefaultAsync(p => p.PlanId == id);
        if (plan == null) return NotFound();

        plan.Name = string.IsNullOrWhiteSpace(dto.Name) ? plan.Name : dto.Name.Trim();
        plan.MaxEmployees = dto.MaxEmployees;
        plan.MaxUsers = dto.MaxUsers;
        plan.MaxBranches = dto.MaxBranches;
        plan.MaxStorageBytes = dto.MaxStorageBytes;

        var codes = dto.FeatureCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToHashSet();
        _db.PlanFeatures.RemoveRange(plan.Features);
        plan.Features = codes.Select(code => new PlanFeature { PlanId = plan.PlanId, FeatureCode = code }).ToList();
        await _db.SaveChangesAsync();

        return Ok(await _db.Plans.AsNoTracking().Where(p => p.PlanId == id).Select(ToDto).SingleAsync());
    }

    private static readonly System.Linq.Expressions.Expression<Func<Plan, PlanDto>> ToDto =
        p => new PlanDto
        {
            PlanId = p.PlanId, Code = p.Code, Name = p.Name,
            MaxEmployees = p.MaxEmployees, MaxUsers = p.MaxUsers,
            MaxBranches = p.MaxBranches, MaxStorageBytes = p.MaxStorageBytes,
            FeatureCodes = p.Features.Where(f => f.IsEnabled).Select(f => f.FeatureCode).ToList()
        };
}
