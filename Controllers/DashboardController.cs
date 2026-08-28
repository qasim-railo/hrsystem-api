using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HRSystem.API.Tenancy;

namespace HRSystem.API.Controllers;

[ApiController, Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _currentTenant;
    public DashboardController(AppDbContext db, ICurrentTenant currentTenant)
    {
        _db = db;
        _currentTenant = currentTenant;
    }

    [HttpGet("widgets")]
    public async Task<ActionResult<DashboardWidgetsDto>> Widgets()
    {
        if (_currentTenant.TenantId is not int tenantId) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().Include(x => x.Plan)
            .SingleOrDefaultAsync(x => x.TenantId == tenantId);
        if (tenant == null) return Forbid();

        var today = DateTime.UtcNow.Date;
        var expiry = today.AddDays(30);
        var permissions = User.Claims.Where(x => x.Type == "permission").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var canViewPeople = permissions.Contains("Users.Manage") || permissions.Contains("Employees.View") || permissions.Contains("Employees.Manage");
        var canViewPayroll = permissions.Contains("Payroll.View") || permissions.Contains("Payroll.Manage") || permissions.Contains("Users.Manage");
        var features = tenant.Plan?.Features.Where(x => x.IsEnabled).Select(x => x.FeatureCode).ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var widgets = new List<DashboardWidgetDto>
        {
            new() { Key = "active-employees", Title = "Active Employees", Icon = "badge", Value = canViewPeople ? await _db.Employees.CountAsync(x => x.Status.ToString() == "Active") : 0, Visible = canViewPeople, SortOrder = 10 },
            new() { Key = "employees-on-leave", Title = "Employees On Leave", Icon = "beach_access", Value = canViewPeople ? await _db.LeaveRequests.CountAsync(x => x.Status == "Approved" && x.StartDate <= today && x.EndDate >= today) : 0, Visible = canViewPeople, SortOrder = 20 },
            new() { Key = "todays-attendance", Title = "Today's Attendance", Icon = "fact_check", Value = canViewPeople ? await _db.Attendance.CountAsync(x => x.Date == today) : 0, Visible = canViewPeople, SortOrder = 30 },
            new() { Key = "pending-approvals", Title = "Pending Approvals", Icon = "pending_actions", Value = canViewPeople ? await _db.ApprovalRequests.CountAsync(x => x.Status == "Pending") : 0, Visible = canViewPeople, SortOrder = 40 },
            new() { Key = "payroll-status", Title = "Approved Payrolls", Icon = "payments", Value = canViewPayroll && features.Contains("BASIC_PAYROLL") ? await _db.Payrolls.CountAsync(x => x.IsApproved) : 0, Visible = canViewPayroll && features.Contains("BASIC_PAYROLL"), SortOrder = 50 },
            new() { Key = "document-expiry", Title = "Documents Expiring Soon", Icon = "description", Value = canViewPeople && features.Contains("EXPIRY_ALERTS") ? await _db.EmploymentDetails.CountAsync(x => (x.VisaExpiry >= today && x.VisaExpiry <= expiry) || (x.LaborCardExpiry >= today && x.LaborCardExpiry <= expiry)) : 0, Visible = canViewPeople && features.Contains("EXPIRY_ALERTS"), SortOrder = 60 },
            new() { Key = "assets-alerts", Title = "Assigned Assets", Icon = "inventory_2", Value = canViewPeople && features.Contains("ASSETS") ? await _db.EmployeeAssets.CountAsync(x => x.Status == "Assigned") : 0, Visible = canViewPeople && features.Contains("ASSETS"), SortOrder = 70 },
            new() { Key = "loans-alerts", Title = "Loan Alerts", Icon = "account_balance", Value = canViewPeople && features.Contains("LOANS") ? 0 : 0, Visible = canViewPeople && features.Contains("LOANS"), SortOrder = 80 }
        };

        return Ok(new DashboardWidgetsDto
        {
            PlanCode = tenant.Plan?.Code ?? string.Empty,
            Roles = User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).Distinct().ToArray(),
            Widgets = widgets.Where(x => x.Visible).OrderBy(x => x.SortOrder).ToArray()
        });
    }
}
