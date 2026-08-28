namespace HRSystem.API.DTOs;

public class OvertimePolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeCategory { get; set; } = "*";
    public string DayType { get; set; } = "Normal Day";
    public string Classification { get; set; } = "OT1";
    public decimal RateMultiplier { get; set; } = 1.25m;
    public int DailyThresholdMinutes { get; set; }
    public int MaximumApprovedMinutes { get; set; }
    public bool ApprovalRequired { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public class SaveOvertimePolicyDto : OvertimePolicyDto { }
