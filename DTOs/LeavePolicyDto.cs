namespace HRSystem.API.DTOs;

public class LeavePolicyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int EntitlementDays { get; set; }
    public string AccrualMethod { get; set; } = "Annual";
    public int CarryForwardLimit { get; set; }
    public bool AllowEncashment { get; set; }
    public int MinimumServiceDays { get; set; }
    public bool DocumentRequired { get; set; }
    public bool ApprovalRequired { get; set; } = true;
    public string EmployeeCategory { get; set; } = "*";
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class LeaveBalanceDto
{
    public string LeaveType { get; set; } = "";
    public int EntitlementDays { get; set; }
    public int UsedDays { get; set; }
    public int CarryForwardDays { get; set; }
    public int RemainingDays { get; set; }
}
