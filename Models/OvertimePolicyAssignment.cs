namespace HRSystem.API.Models;

public class OvertimePolicyAssignment : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int OvertimePolicyId { get; set; }
    public OvertimePolicy OvertimePolicy { get; set; } = null!;
    public string Scope { get; set; } = "All";
    public int? TargetId { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
