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
    private static readonly PlanModuleDto[] AvailableModules =
    [
        new() { Code = "EMPLOYEE_MANAGEMENT", Name = "Employee Management" },
        new() { Code = "DOCUMENTS", Name = "Documents" },
        new() { Code = "LEAVE", Name = "Leave Management" },
        new() { Code = "ATTENDANCE", Name = "Attendance" },
        new() { Code = "SHIFTS", Name = "Shifts" },
        new() { Code = "BASIC_PAYROLL", Name = "Payroll" },
        new() { Code = "PAYSLIPS", Name = "Payslips" },
        new() { Code = "STANDARD_REPORTS", Name = "Standard Reports" },
        new() { Code = "EMPLOYEE_SELF_SERVICE", Name = "Employee Self-Service" },
        new() { Code = "LOANS", Name = "Loans" },
        new() { Code = "OVERTIME", Name = "Overtime" },
        new() { Code = "ASSETS", Name = "Asset Management" },
        new() { Code = "GRATUITY", Name = "Gratuity" },
        new() { Code = "FINAL_SETTLEMENT", Name = "Final Settlement" },
        new() { Code = "ADVANCED_REPORTS", Name = "Advanced Reports" },
        new() { Code = "CUSTOM_ROLES", Name = "Custom Roles" },
        new() { Code = "WORKFLOWS", Name = "Approval Workflows" },
        new() { Code = "ORGANIZATION_CHART", Name = "Organization Chart" },
        new() { Code = "EXPIRY_ALERTS", Name = "Document Expiry Alerts" },
        new() { Code = "ADVANCED_AUDIT", Name = "Advanced Audit" }
    ];
    private static readonly HashSet<string> AvailableModuleCodes = AvailableModules
        .Select(module => module.Code)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
    private readonly AppDbContext _db;

    public PlansController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlanDto>>> GetPlans()
        => Ok(await _db.Plans.AsNoTracking().OrderBy(p => p.PlanId).Select(ToDto).ToListAsync());

    [HttpGet("modules")]
    public ActionResult<IEnumerable<PlanModuleDto>> GetAvailableModules() => Ok(AvailableModules);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlanDto>> GetPlan(int id)
    {
        var plan = await _db.Plans.AsNoTracking().Where(p => p.PlanId == id).Select(ToDto).SingleOrDefaultAsync();
        return plan == null ? NotFound() : Ok(plan);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlanDto>> UpdatePlan(int id, UpdatePlanDto dto)
    {
        var validationError = Validate(dto);
        if (validationError != null) return BadRequest(validationError);

        var plan = await _db.Plans.Include(p => p.Features).SingleOrDefaultAsync(p => p.PlanId == id);
        if (plan == null) return NotFound();

        plan.Name = dto.Name!.Trim();
        plan.MaxEmployees = dto.MaxEmployees;
        plan.MaxUsers = dto.MaxUsers;
        plan.MaxBranches = dto.MaxBranches;
        plan.MaxStorageBytes = dto.MaxStorageBytes;

        var codes = NormalizeFeatureCodes(dto.FeatureCodes);
        _db.PlanFeatures.RemoveRange(plan.Features);
        plan.Features = codes.Select(code => new PlanFeature { PlanId = plan.PlanId, FeatureCode = code }).ToList();
        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            Action = "PlanUpdated",
            Entity = nameof(Plan),
            EntityId = plan.PlanId.ToString(),
            UserId = User.Identity?.Name ?? "unknown",
            Details = $"Plan '{plan.Code}' updated with {codes.Count} enabled module(s)."
        });
        await _db.SaveChangesAsync();

        return Ok(await _db.Plans.AsNoTracking().Where(p => p.PlanId == id).Select(ToDto).SingleAsync());
    }

    [HttpPost]
    public async Task<ActionResult<PlanDto>> CreatePlan(CreatePlanDto dto)
    {
        var validationError = Validate(dto);
        if (validationError != null) return BadRequest(validationError);
        if (string.IsNullOrWhiteSpace(dto.Code)) return BadRequest("A plan code is required.");

        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _db.Plans.AnyAsync(plan => plan.Code == code))
            return Conflict($"A plan with code '{code}' already exists.");

        var plan = new Plan
        {
            Code = code,
            Name = dto.Name!.Trim(),
            MaxEmployees = dto.MaxEmployees,
            MaxUsers = dto.MaxUsers,
            MaxBranches = dto.MaxBranches,
            MaxStorageBytes = dto.MaxStorageBytes,
            Features = NormalizeFeatureCodes(dto.FeatureCodes).Select(featureCode => new PlanFeature { FeatureCode = featureCode }).ToList()
        };
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync();
        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            Action = "PlanCreated",
            Entity = nameof(Plan),
            EntityId = plan.PlanId.ToString(),
            UserId = User.Identity?.Name ?? "unknown",
            Details = $"Plan '{plan.Code}' created with {plan.Features.Count} enabled module(s)."
        });
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlan), new { id = plan.PlanId }, await _db.Plans.AsNoTracking().Where(p => p.PlanId == plan.PlanId).Select(ToDto).SingleAsync());
    }

    private static string? Validate(UpdatePlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return "A plan name is required.";
        if (dto.MaxEmployees < 0 || dto.MaxUsers < 0 || dto.MaxBranches < 0 || dto.MaxStorageBytes < 0)
            return "Plan limits cannot be negative.";

        var invalidCodes = NormalizeFeatureCodes(dto.FeatureCodes).Where(code => !AvailableModuleCodes.Contains(code)).ToArray();
        return invalidCodes.Length > 0 ? $"Unsupported module codes: {string.Join(", ", invalidCodes)}." : null;
    }

    private static HashSet<string> NormalizeFeatureCodes(IEnumerable<string> featureCodes)
        => featureCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static readonly System.Linq.Expressions.Expression<Func<Plan, PlanDto>> ToDto =
        p => new PlanDto
        {
            PlanId = p.PlanId, Code = p.Code, Name = p.Name,
            MaxEmployees = p.MaxEmployees, MaxUsers = p.MaxUsers,
            MaxBranches = p.MaxBranches, MaxStorageBytes = p.MaxStorageBytes,
            FeatureCodes = p.Features.Where(f => f.IsEnabled).Select(f => f.FeatureCode).ToList()
        };
}
