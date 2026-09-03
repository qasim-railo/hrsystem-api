namespace HRSystem.API.Models;

public class OvertimeType : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Eligibility { get; set; } = "All";
    public string CalculationMethod { get; set; } = "Multiplier";
    public decimal RateMultiplier { get; set; } = 1m;
    public int MaximumMinutes { get; set; }
    public bool ApprovalRequired { get; set; }
    public int? PayrollComponentId { get; set; }
    public PayrollComponent? PayrollComponent { get; set; }
    public bool IsActive { get; set; } = true;
}
