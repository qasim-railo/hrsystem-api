using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/overtime-policies")]
public class OvertimePoliciesController : ControllerBase
{
    private readonly AppDbContext _db;
    public OvertimePoliciesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OvertimePolicyDto>>> List()
    {
        if (!await _db.OvertimePolicies.AnyAsync())
        {
            _db.OvertimePolicies.AddRange(
                new OvertimePolicy { Name = "Normal day OT1", DayType = "Normal Day", Classification = "OT1", RateMultiplier = 1.25m, MaximumApprovedMinutes = 120 },
                new OvertimePolicy { Name = "Weekly off OT2", DayType = "Weekly Off", Classification = "OT2", RateMultiplier = 1.5m, MaximumApprovedMinutes = 480 },
                new OvertimePolicy { Name = "Holiday OT2", DayType = "Holiday", Classification = "OT2", RateMultiplier = 2m, MaximumApprovedMinutes = 480 });
            await _db.SaveChangesAsync();
        }
        return Ok(await _db.OvertimePolicies.AsNoTracking().OrderBy(x => x.DayType).ThenBy(x => x.Name).Select(MapExpression).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<OvertimePolicyDto>> Create(SaveOvertimePolicyDto dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);
        var policy = new OvertimePolicy();
        Apply(policy, dto);
        _db.OvertimePolicies.Add(policy);
        await _db.SaveChangesAsync();
        return Ok(Map(policy));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<OvertimePolicyDto>> Update(int id, SaveOvertimePolicyDto dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);
        var policy = await _db.OvertimePolicies.SingleOrDefaultAsync(x => x.Id == id);
        if (policy == null) return NotFound();
        Apply(policy, dto);
        await _db.SaveChangesAsync();
        return Ok(Map(policy));
    }

    private static string? Validate(SaveOvertimePolicyDto d) =>
        string.IsNullOrWhiteSpace(d.Name) ? "Name is required." :
        d.RateMultiplier <= 0 || d.DailyThresholdMinutes < 0 || d.MaximumApprovedMinutes < 0 ? "Overtime values must be valid." :
        !new[] { "Normal Day", "Weekly Off", "Holiday", "Special", "Any" }.Contains(d.DayType) ? "Invalid day type." :
        !new[] { "OT1", "OT2" }.Contains(d.Classification) ? "Classification must be OT1 or OT2." : null;
    private static void Apply(OvertimePolicy x, SaveOvertimePolicyDto d)
    {
        x.Name = d.Name.Trim(); x.EmployeeCategory = d.EmployeeCategory?.Trim() ?? "*"; x.DayType = d.DayType;
        x.Classification = d.Classification; x.RateMultiplier = d.RateMultiplier; x.DailyThresholdMinutes = d.DailyThresholdMinutes;
        x.MaximumApprovedMinutes = d.MaximumApprovedMinutes; x.ApprovalRequired = d.ApprovalRequired; x.EffectiveFrom = d.EffectiveFrom.Date;
        x.EffectiveTo = d.EffectiveTo?.Date; x.IsActive = d.IsActive;
    }
    private static OvertimePolicyDto Map(OvertimePolicy x) => new() { Id = x.Id, Name = x.Name, EmployeeCategory = x.EmployeeCategory, DayType = x.DayType, Classification = x.Classification, RateMultiplier = x.RateMultiplier, DailyThresholdMinutes = x.DailyThresholdMinutes, MaximumApprovedMinutes = x.MaximumApprovedMinutes, ApprovalRequired = x.ApprovalRequired, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, IsActive = x.IsActive };
    private static readonly System.Linq.Expressions.Expression<Func<OvertimePolicy, OvertimePolicyDto>> MapExpression = x => new OvertimePolicyDto { Id = x.Id, Name = x.Name, EmployeeCategory = x.EmployeeCategory, DayType = x.DayType, Classification = x.Classification, RateMultiplier = x.RateMultiplier, DailyThresholdMinutes = x.DailyThresholdMinutes, MaximumApprovedMinutes = x.MaximumApprovedMinutes, ApprovalRequired = x.ApprovalRequired, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, IsActive = x.IsActive };
}
