namespace HRSystem.API.Models;

public class ApprovalAction : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ApprovalRequestId { get; set; }
    public ApprovalRequest Request { get; set; } = null!;
    public int StepOrder { get; set; }
    public int ActionByUserId { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
