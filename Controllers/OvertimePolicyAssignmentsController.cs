using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/overtime-policy-assignments")]
public class OvertimePolicyAssignmentsController : ControllerBase
{
    private static readonly string[] Scopes = ["All", "Company", "Branch", "Department", "Category", "Designation", "Employee"];
    private readonly AppDbContext _db;
    public OvertimePolicyAssignmentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OvertimePolicyAssignmentDto>>> List() =>
        Ok(await _db.OvertimePolicyAssignments.AsNoTracking().OrderBy(x => x.Scope).ThenBy(x => x.EffectiveFrom).Select(MapExpression).ToListAsync());

    [HttpGet("targets/{scope}")]
    public async Task<ActionResult<IEnumerable<object>>> Targets(string scope) => scope switch
    {
        "Company" => Ok(await _db.Companies.AsNoTracking().OrderBy(x => x.Name).Select(x => new { id = x.CompanyId, x.Name }).ToListAsync()),
        "Branch" => Ok(await _db.Branches.AsNoTracking().OrderBy(x => x.Name).Select(x => new { id = x.BranchId, x.Name }).ToListAsync()),
        "Department" => Ok(await _db.Department.AsNoTracking().OrderBy(x => x.Name).Select(x => new { id = x.DepartmentId, x.Name }).ToListAsync()),
        "Category" => Ok(await _db.EmployeeCategories.AsNoTracking().OrderBy(x => x.SortOrder).Select(x => new { id = x.EmployeeCategoryId, x.Name }).ToListAsync()),
        "Designation" => Ok(await _db.Positions.AsNoTracking().OrderBy(x => x.Name).Select(x => new { id = x.PositionId, x.Name }).ToListAsync()),
        "Employee" => Ok(await _db.Employees.AsNoTracking().OrderBy(x => x.FirstName).ThenBy(x => x.LastName).Select(x => new { id = x.EmployeeId, Name = x.FirstName + " " + x.LastName }).ToListAsync()),
        "All" => Ok(Array.Empty<object>()),
        _ => BadRequest("Invalid assignment scope.")
    };

    [HttpPost]
    public async Task<ActionResult<OvertimePolicyAssignmentDto>> Create(SaveOvertimePolicyAssignmentDto dto)
    {
        var error = await Validate(dto, null);
        if (error != null) return BadRequest(error);
        var item = new OvertimePolicyAssignment(); Apply(item, dto); _db.Add(item);
        await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OvertimePolicyAssignmentDto>> Update(int id, SaveOvertimePolicyAssignmentDto dto)
    {
        var item = await _db.OvertimePolicyAssignments.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        var error = await Validate(dto, id);
        if (error != null) return BadRequest(error);
        Apply(item, dto); await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    private async Task<string?> Validate(SaveOvertimePolicyAssignmentDto dto, int? id)
    {
        if (!Scopes.Contains(dto.Scope)) return "Invalid assignment scope.";
        if (!await _db.OvertimePolicies.AnyAsync(x => x.Id == dto.OvertimePolicyId)) return "Select a tenant overtime policy.";
        if ((dto.Scope == "All") != !dto.TargetId.HasValue) return dto.Scope == "All" ? "All scope cannot have a target." : "A target is required for this scope.";
        if (dto.EffectiveTo < dto.EffectiveFrom) return "Effective to date cannot precede effective from date.";
        if (dto.Scope != "All" && !await TargetExists(dto.Scope, dto.TargetId!.Value)) return "Select a target in this tenant.";
        return null;
    }

    private Task<bool> TargetExists(string scope, int id) => scope switch
    {
        "Company" => _db.Companies.AnyAsync(x => x.CompanyId == id),
        "Branch" => _db.Branches.AnyAsync(x => x.BranchId == id),
        "Department" => _db.Department.AnyAsync(x => x.DepartmentId == id),
        "Category" => _db.EmployeeCategories.AnyAsync(x => x.EmployeeCategoryId == id),
        "Designation" => _db.Positions.AnyAsync(x => x.PositionId == id),
        "Employee" => _db.Employees.AnyAsync(x => x.EmployeeId == id),
        _ => Task.FromResult(false)
    };
    private static void Apply(OvertimePolicyAssignment x, SaveOvertimePolicyAssignmentDto d)
    {
        x.OvertimePolicyId = d.OvertimePolicyId; x.Scope = d.Scope; x.TargetId = d.TargetId;
        x.EffectiveFrom = d.EffectiveFrom.Date; x.EffectiveTo = d.EffectiveTo?.Date; x.IsActive = d.IsActive;
    }
    private static OvertimePolicyAssignmentDto Map(OvertimePolicyAssignment x) => new() { Id = x.Id, OvertimePolicyId = x.OvertimePolicyId, Scope = x.Scope, TargetId = x.TargetId, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, IsActive = x.IsActive };
    private static readonly System.Linq.Expressions.Expression<Func<OvertimePolicyAssignment, OvertimePolicyAssignmentDto>> MapExpression = x => new OvertimePolicyAssignmentDto { Id = x.Id, OvertimePolicyId = x.OvertimePolicyId, Scope = x.Scope, TargetId = x.TargetId, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, IsActive = x.IsActive };
}
