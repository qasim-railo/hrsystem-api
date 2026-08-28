namespace HRSystem.API.Models;

public class ApprovalStep
{
    public int Id { get; set; }
    public int ApprovalWorkflowId { get; set; }
    public ApprovalWorkflow Workflow { get; set; } = null!;
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public string ApprovalMode { get; set; } = "Sequential";
    public int? EscalationAfterHours { get; set; }
}
