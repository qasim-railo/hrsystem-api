namespace HRSystem.API.DTOs;

public class PlatformTenantDto
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LifecycleStatus { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public int? TrialDaysRemaining { get; set; }
    public string BillingStatus { get; set; } = string.Empty;
    public long StorageUsedBytes { get; set; }
    public long StorageLimitBytes { get; set; }
    public int UserCount { get; set; }
    public int EmployeeCount { get; set; }
    public int CompanyCount { get; set; }

    public static int? CalculateTrialDaysRemaining(DateTime? trialEndDate, string? status = null, string? lifecycleStatus = null)
    {
        if (!trialEndDate.HasValue)
            return null;

        var activeTrialStatus = string.Equals(status, "Trial", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase)
            || string.Equals(lifecycleStatus, "Onboarding", StringComparison.OrdinalIgnoreCase)
            || string.Equals(lifecycleStatus, "Trial", StringComparison.OrdinalIgnoreCase);

        if (!activeTrialStatus)
            return null;

        var remainingDays = (trialEndDate.Value.Date - DateTime.UtcNow.Date).Days;
        return remainingDays < 0 ? 0 : remainingDays;
    }
}

public class PlatformStatisticsDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TrialTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int TotalUsers { get; set; }
    public int TotalEmployees { get; set; }
    public long TotalStorageUsedBytes { get; set; }
}
