using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRSystem.API.Controllers;

[ApiController, Authorize]
[Route("api/manager-portal")]
public class ManagerPortalController : ControllerBase
{
    private readonly AppDbContext _db;
    public ManagerPortalController(AppDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<ActionResult<ManagerPortalDto>> Dashboard()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var manager = await _db.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.Email == username);
        var teamQuery = _db.Employees.AsNoTracking();
        if (manager != null)
            teamQuery = teamQuery.Where(x => x.DepartmentId == manager.DepartmentId);
        else if (!User.Claims.Any(x => x.Type == "permission" && x.Value == "Users.Manage"))
            return Forbid();

        var teamIds = await teamQuery.Select(x => x.EmployeeId).ToArrayAsync();
        var today = DateTime.UtcNow.Date;
        var weekEnd = today.AddDays(7);
        var expiry = today.AddDays(30);
        return Ok(new ManagerPortalDto
        {
            Team = await teamQuery.Include(x => x.Department).Select(x => (object)new { x.EmployeeId, x.EmployeeCode, x.FirstName, x.LastName, x.Email, Department = x.Department.Name }).ToArrayAsync(),
            PendingLeaveRequests = await _db.LeaveRequests.AsNoTracking().Where(x => teamIds.Contains(x.EmployeeId) && x.Status == "Pending").OrderBy(x => x.StartDate).Select(x => (object)new { x.Id, x.EmployeeId, x.LeaveType, x.StartDate, x.EndDate, x.Reason }).ToArrayAsync(),
            PendingAttendanceCorrections = await _db.ApprovalRequests.AsNoTracking().Where(x => x.Status == "Pending" && x.Module == "Attendance").Select(x => (object)new { x.Id, x.RequestType, x.Reference, x.CreatedAt }).ToArrayAsync(),
            PendingOvertime = await _db.ApprovalRequests.AsNoTracking().Where(x => x.Status == "Pending" && x.Module == "Overtime").Select(x => (object)new { x.Id, x.RequestType, x.Reference, x.CreatedAt }).ToArrayAsync(),
            EmployeesOnLeave = await _db.LeaveRequests.AsNoTracking().Where(x => teamIds.Contains(x.EmployeeId) && x.Status == "Approved" && x.StartDate <= today && x.EndDate >= today).Select(x => (object)new { x.EmployeeId, x.LeaveType, x.StartDate, x.EndDate }).ToArrayAsync(),
            TodaysTeamAttendance = await _db.Attendance.AsNoTracking().Where(x => teamIds.Contains(x.EmployeeId) && x.Date == today).Select(x => (object)new { x.EmployeeId, x.CheckIn, x.CheckOut, x.OT1, x.OT2 }).ToArrayAsync(),
            TeamCalendar = await _db.LeaveRequests.AsNoTracking().Where(x => teamIds.Contains(x.EmployeeId) && x.Status == "Approved" && x.StartDate >= today && x.StartDate <= weekEnd).Select(x => (object)new { x.EmployeeId, x.LeaveType, x.StartDate, x.EndDate }).ToArrayAsync(),
            DocumentExpiryAlerts = await _db.Employees.AsNoTracking().Include(x => x.EmploymentDetail).Where(x => teamIds.Contains(x.EmployeeId) && ((x.EmploymentDetail != null && x.EmploymentDetail.VisaExpiry <= expiry && x.EmploymentDetail.VisaExpiry >= today) || (x.EmploymentDetail != null && x.EmploymentDetail.LaborCardExpiry <= expiry && x.EmploymentDetail.LaborCardExpiry >= today))).Select(x => (object)new { x.EmployeeId, x.EmployeeCode, x.FirstName, x.LastName, VisaExpiry = x.EmploymentDetail!.VisaExpiry, LaborCardExpiry = x.EmploymentDetail.LaborCardExpiry }).ToArrayAsync()
        });
    }
}
