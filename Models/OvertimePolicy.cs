namespace HRSystem.API.Models;

public class OvertimePolicy : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeCategory { get; set; } = "*";
    public string DayType { get; set; } = "Normal Day";
    public string Classification { get; set; } = "OT1";
    public int? OvertimeTypeId { get; set; }
    public OvertimeType? OvertimeType { get; set; }
    public decimal RateMultiplier { get; set; } = 1.25m;
    public int DailyThresholdMinutes { get; set; }
    public int MaximumApprovedMinutes { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
