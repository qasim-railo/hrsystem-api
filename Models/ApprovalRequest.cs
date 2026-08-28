namespace HRSystem.API.Models;

public class ApprovalRequest : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int ApprovalWorkflowId { get; set; }
    public ApprovalWorkflow Workflow { get; set; } = null!;
    public string Module { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public int RequestedByUserId { get; set; }
    public string Status { get; set; } = "Pending";
    public int CurrentStepOrder { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public ICollection<ApprovalAction> Actions { get; set; } = new List<ApprovalAction>();
}
