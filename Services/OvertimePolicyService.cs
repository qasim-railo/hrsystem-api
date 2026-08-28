using HRSystem.API.Data;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class OvertimePolicyService
{
    private readonly AppDbContext _db;
    public OvertimePolicyService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<OvertimePolicy>> GetActiveAsync(int employeeId, DateTime date)
    {
        var category = await _db.Employees.Where(x => x.EmployeeId == employeeId).Select(x => x.EmploymentDetail!.Category).SingleOrDefaultAsync() ?? "*";
        var dayType = date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday ? "Weekly Off" : "Normal Day";
        return await _db.OvertimePolicies.Where(x => x.IsActive && x.EffectiveFrom <= date && (x.EffectiveTo == null || x.EffectiveTo >= date) &&
            (x.EmployeeCategory == "*" || x.EmployeeCategory == category) && (x.DayType == dayType || x.DayType == "Any"))
            .OrderBy(x => x.EmployeeCategory == "*" ? 1 : 0).ThenBy(x => x.DailyThresholdMinutes).ToListAsync();
    }

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
