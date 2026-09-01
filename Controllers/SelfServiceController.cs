using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRSystem.API.Controllers;

[ApiController, Authorize]
[Route("api/self-service")]
public class SelfServiceController : ControllerBase
{
    private readonly AppDbContext _db;
    public SelfServiceController(AppDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<ActionResult<SelfServiceDto>> Dashboard()
    {
        var username = User.FindFirstValue(ClaimTypes.Name);
        var employee = await _db.Employees.Include(x => x.Company).Include(x => x.Department).Include(x => x.EmploymentDetail)
            .SingleOrDefaultAsync(x => x.Email == username);
        if (employee == null) return NotFound("No employee profile is linked to this account.");

        return Ok(new SelfServiceDto
        {
            Profile = new { employee.EmployeeId, employee.EmployeeCode, employee.FirstName, employee.LastName, employee.Email, Company = employee.Company.Name, Department = employee.Department.Name, Category = employee.EmploymentDetail!.Category, Designation = employee.EmploymentDetail.OfferDesignation, employee.EmploymentDetail.JoiningDate },
            Attendance = await _db.Attendance.AsNoTracking().Where(x => x.EmployeeId == employee.EmployeeId).OrderByDescending(x => x.Date).Take(50).Select(x => (object)new { x.Date, x.CheckIn, x.CheckOut, x.OT1, x.OT2 }).ToArrayAsync(),
            Leave = await _db.LeaveRequests.AsNoTracking().Where(x => x.EmployeeId == employee.EmployeeId).OrderByDescending(x => x.StartDate).Take(50).Select(x => (object)new { x.LeaveType, x.StartDate, x.EndDate, x.Status, x.Reason }).ToArrayAsync(),
            Payslips = await _db.Payrolls.AsNoTracking().Where(x => x.EmployeeId == employee.EmployeeId && x.IsApproved).OrderByDescending(x => x.Month).Take(24).Select(x => (object)new { x.Id, x.Month, x.NetSalary, x.OT1Hours, x.OT2Hours }).ToArrayAsync(),
            Documents = await _db.EmployeeDocuments.AsNoTracking().Where(x => x.EmployeeId == employee.EmployeeId).OrderByDescending(x => x.UploadedAt).Select(x => (object)new { x.Id, x.FileName, x.FileType, x.UploadedAt }).ToArrayAsync(),
            Assets = await _db.EmployeeAssets.AsNoTracking().Include(x => x.Asset).Where(x => x.EmployeeId == employee.EmployeeId && x.ReturnedDate == null).Select(x => (object)new { x.Id, Asset = x.Asset.Name, x.AssignedDate, x.Status }).ToArrayAsync(),
            Requests = await _db.LeaveRequests.AsNoTracking().Where(x => x.EmployeeId == employee.EmployeeId).OrderByDescending(x => x.StartDate).Take(20).Select(x => (object)new { Type = "Leave", x.StartDate, x.EndDate, x.Status }).ToArrayAsync()
        });
    }
}

