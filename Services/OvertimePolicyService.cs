using HRSystem.API.Data;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class OvertimePolicyService
{
    private readonly AppDbContext _db;
    private readonly TenantWorkingDayService _workingDays;
    public OvertimePolicyService(AppDbContext db, TenantWorkingDayService workingDays) { _db = db; _workingDays = workingDays; }

    public async Task<IReadOnlyList<OvertimePolicy>> GetActiveAsync(int employeeId, DateTime date)
    {
        var employee = await _db.Employees.Include(x => x.EmploymentDetail).SingleOrDefaultAsync(x => x.EmployeeId == employeeId);
        if (employee == null) return [];
        var dayType = await _workingDays.IsWorkingDayAsync(date) ? "Normal Day" : "Weekly Off";
        var assignments = await _db.OvertimePolicyAssignments.Where(x => x.IsActive && x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date))
            .Where(x => x.Scope == "All" ||
                (x.Scope == "Company" && x.TargetId == employee.CompanyId) ||
                (x.Scope == "Branch" && x.TargetId == employee.BranchId) ||
                (x.Scope == "Department" && x.TargetId == employee.DepartmentId) ||
                (x.Scope == "Category" && x.TargetId == employee.EmploymentDetail!.EmployeeCategoryId) ||
                (x.Scope == "Designation" && x.TargetId == employee.PositionId) ||
                (x.Scope == "Employee" && x.TargetId == employee.EmployeeId))
            .ToListAsync();
        if (assignments.Count > 0)
        {
            var highestPriority = assignments.Min(x => Priority(x.Scope));
            var policyIds = assignments.Where(x => Priority(x.Scope) == highestPriority).Select(x => x.OvertimePolicyId).Distinct();
            return await _db.OvertimePolicies.Where(x => policyIds.Contains(x.Id) && x.IsActive && x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date) && (x.DayType == dayType || x.DayType == "Any"))
                .OrderBy(x => x.DailyThresholdMinutes).ToListAsync();
        }
        var category = employee.EmploymentDetail?.Category ?? "*";
        return await _db.OvertimePolicies.Where(x => x.IsActive && x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date) && (x.EmployeeCategory == "*" || x.EmployeeCategory == category) && (x.DayType == dayType || x.DayType == "Any"))
            .OrderBy(x => x.EmployeeCategory == "*" ? 1 : 0).ThenBy(x => x.DailyThresholdMinutes).ToListAsync();
    }

    private static int Priority(string scope) => scope switch { "Employee" => 0, "Designation" => 1, "Category" => 2, "Department" => 3, "Branch" => 4, "Company" => 5, _ => 6 };

    public static (int ot1, int ot2) Allocate(int minutes, IReadOnlyList<OvertimePolicy> policies)
    {
        if (minutes <= 0 || policies.Count == 0) return (0, 0);
        var remaining = minutes;
        var ot1 = 0; var ot2 = 0;
        foreach (var policy in policies.OrderBy(x => x.DailyThresholdMinutes))
        {
            var eligible = Math.Max(0, remaining - policy.DailyThresholdMinutes);
            if (policy.MaximumApprovedMinutes > 0) eligible = Math.Min(eligible, policy.MaximumApprovedMinutes);
            if (policy.Classification.Equals("OT2", StringComparison.OrdinalIgnoreCase)) ot2 += eligible;
            else ot1 += eligible;
            remaining -= eligible;
            if (remaining <= 0) break;
        }
        return (ot1, ot2);
    }
}
