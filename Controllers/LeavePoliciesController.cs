using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/leave-policies")]
public class LeavePoliciesController : ControllerBase
{
    private readonly AppDbContext _db;
    public LeavePoliciesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeavePolicyDto>>> List()
    {
        if (!await _db.TenantLeaveTypes.AnyAsync())
        {
            _db.TenantLeaveTypes.AddRange(
                new TenantLeaveType { Name = "Annual", DefaultDays = 21, AccrualMethod = "Annual" },
                new TenantLeaveType { Name = "Sick", DefaultDays = 14, AccrualMethod = "Annual", DocumentRequired = true },
                new TenantLeaveType { Name = "Emergency", DefaultDays = 3, AccrualMethod = "Annual" });
            await _db.SaveChangesAsync();
        }
        return Ok(await _db.TenantLeaveTypes.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new LeavePolicyDto { Id = x.Id, Name = x.Name, EntitlementDays = x.DefaultDays, AccrualMethod = x.AccrualMethod, CarryForwardLimit = x.CarryForwardLimit, AllowEncashment = x.AllowEncashment, MinimumServiceDays = x.MinimumServiceDays, DocumentRequired = x.DocumentRequired, ApprovalRequired = x.ApprovalRequired, EmployeeCategory = x.EmployeeCategory, EffectiveFrom = x.EffectiveFrom, EffectiveTo = x.EffectiveTo, IsActive = x.IsActive }).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<LeavePolicyDto>> Create(LeavePolicyDto dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);
        var item = new TenantLeaveType();
        Apply(item, dto);
        _db.TenantLeaveTypes.Add(item);
        await _db.SaveChangesAsync();
        dto.Id = item.Id;
        return Ok(dto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LeavePolicyDto>> Update(int id, LeavePolicyDto dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);
        var item = await _db.TenantLeaveTypes.SingleOrDefaultAsync(x => x.Id == id);
        if (item == null) return NotFound();
        Apply(item, dto);
        await _db.SaveChangesAsync();
        dto.Id = id;
        return Ok(dto);
    }

    [HttpGet("balance/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> Balance(int employeeId)
    {
        var year = DateTime.UtcNow.Year;
        var requests = await _db.LeaveRequests.Where(x => x.EmployeeId == employeeId && x.Status == "Approved" && x.StartDate.Year == year).ToListAsync();
        var policies = await _db.TenantLeaveTypes.Where(x => x.IsActive).ToListAsync();
        return Ok(policies.Select(x =>
        {
            var used = requests.Where(r => r.LeaveType == x.Name).Sum(r => (r.EndDate.Date - r.StartDate.Date).Days + 1);
            var carry = Math.Max(0, x.CarryForwardLimit);
            return new LeaveBalanceDto { LeaveType = x.Name, EntitlementDays = x.DefaultDays, UsedDays = used, CarryForwardDays = carry, RemainingDays = Math.Max(0, x.DefaultDays + carry - used) };
        }));
    }

    private static string? Validate(LeavePolicyDto d) => string.IsNullOrWhiteSpace(d.Name) ? "Name is required." : d.EntitlementDays < 0 || d.CarryForwardLimit < 0 || d.MinimumServiceDays < 0 ? "Leave policy values must be non-negative." : !new[] { "Annual", "Monthly", "Daily", "OnJoining" }.Contains(d.AccrualMethod) ? "Invalid accrual method." : null;
    private static void Apply(TenantLeaveType x, LeavePolicyDto d) { x.Name = d.Name.Trim(); x.DefaultDays = d.EntitlementDays; x.AccrualMethod = d.AccrualMethod; x.CarryForwardLimit = d.CarryForwardLimit; x.AllowEncashment = d.AllowEncashment; x.MinimumServiceDays = d.MinimumServiceDays; x.DocumentRequired = d.DocumentRequired; x.ApprovalRequired = d.ApprovalRequired; x.EmployeeCategory = string.IsNullOrWhiteSpace(d.EmployeeCategory) ? "*" : d.EmployeeCategory.Trim(); x.EffectiveFrom = d.EffectiveFrom.Date; x.EffectiveTo = d.EffectiveTo?.Date; x.IsActive = d.IsActive; }
}
