using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/overtime-types")]
public class OvertimeTypesController : ControllerBase
{
    private readonly AppDbContext _db;
    public OvertimeTypesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OvertimeTypeDto>>> List()
    {
        await EnsureDefaultsAsync();
        return Ok(await _db.OvertimeTypes.AsNoTracking().OrderBy(x => x.Name).Select(MapExpression).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<OvertimeTypeDto>> Create(SaveOvertimeTypeDto dto)
    {
        var error = await Validate(dto, null);
        if (error != null) return BadRequest(error);
        var item = new OvertimeType(); Apply(item, dto); _db.OvertimeTypes.Add(item);
        await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OvertimeTypeDto>> Update(int id, SaveOvertimeTypeDto dto)
    {
        var item = await _db.OvertimeTypes.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        var error = await Validate(dto, id);
        if (error != null) return BadRequest(error);
        Apply(item, dto); await _db.SaveChangesAsync(); return Ok(Map(item));
    }

    private async Task EnsureDefaultsAsync()
    {
        if (await _db.OvertimeTypes.AnyAsync()) return;
        _db.OvertimeTypes.AddRange(
            new OvertimeType { Code = "OT1", Name = "Regular OT1", RateMultiplier = 1.25m, MaximumMinutes = 120 },
            new OvertimeType { Code = "OT2", Name = "Regular OT2", RateMultiplier = 1.5m },
            new OvertimeType { Code = "REST_DAY", Name = "Rest Day", RateMultiplier = 1.5m, MaximumMinutes = 480 },
            new OvertimeType { Code = "HOLIDAY", Name = "Public Holiday", RateMultiplier = 2m, MaximumMinutes = 480 },
            new OvertimeType { Code = "SPECIAL_DAY", Name = "Special Holiday", RateMultiplier = 2m, MaximumMinutes = 480 });
        await _db.SaveChangesAsync();
    }

    private async Task<string?> Validate(SaveOvertimeTypeDto dto, int? id)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)) return "Code and name are required.";
        if (dto.CalculationMethod is not ("Multiplier" or "Fixed")) return "Calculation method must be Multiplier or Fixed.";
        if (dto.RateMultiplier <= 0 || dto.MaximumMinutes < 0) return "Rate and maximum minutes must be valid.";
        if (await _db.OvertimeTypes.AnyAsync(x => x.Id != id && x.Code == dto.Code.Trim().ToUpperInvariant())) return "An overtime type with this code already exists.";
        if (dto.PayrollComponentId.HasValue && !await _db.PayrollComponents.AnyAsync(x => x.Id == dto.PayrollComponentId && x.IsActive)) return "Select an active tenant payroll component.";
        return null;
    }
    private static void Apply(OvertimeType x, SaveOvertimeTypeDto d)
    {
        x.Code = d.Code.Trim().ToUpperInvariant(); x.Name = d.Name.Trim(); x.Eligibility = d.Eligibility?.Trim() ?? "All";
        x.CalculationMethod = d.CalculationMethod; x.RateMultiplier = d.RateMultiplier; x.MaximumMinutes = d.MaximumMinutes;
        x.ApprovalRequired = d.ApprovalRequired; x.PayrollComponentId = d.PayrollComponentId; x.IsActive = d.IsActive;
    }
    private static OvertimeTypeDto Map(OvertimeType x) => new() { Id = x.Id, Code = x.Code, Name = x.Name, Eligibility = x.Eligibility, CalculationMethod = x.CalculationMethod, RateMultiplier = x.RateMultiplier, MaximumMinutes = x.MaximumMinutes, ApprovalRequired = x.ApprovalRequired, PayrollComponentId = x.PayrollComponentId, IsActive = x.IsActive };
    private static readonly System.Linq.Expressions.Expression<Func<OvertimeType, OvertimeTypeDto>> MapExpression = x => new OvertimeTypeDto { Id = x.Id, Code = x.Code, Name = x.Name, Eligibility = x.Eligibility, CalculationMethod = x.CalculationMethod, RateMultiplier = x.RateMultiplier, MaximumMinutes = x.MaximumMinutes, ApprovalRequired = x.ApprovalRequired, PayrollComponentId = x.PayrollComponentId, IsActive = x.IsActive };
}
