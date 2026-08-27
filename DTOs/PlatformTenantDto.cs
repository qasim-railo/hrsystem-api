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
    public string BillingStatus { get; set; } = string.Empty;
    public long StorageUsedBytes { get; set; }
    public int UserCount { get; set; }
    public int EmployeeCount { get; set; }
    public int CompanyCount { get; set; }
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
