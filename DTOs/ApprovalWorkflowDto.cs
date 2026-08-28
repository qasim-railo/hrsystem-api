namespace HRSystem.API.DTOs;

public class ApprovalWorkflowDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ApprovalStepDto> Steps { get; set; } = new();
}

public class ApprovalStepDto
{
    public int Id { get; set; }
    public int StepOrder { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public string ApprovalMode { get; set; } = "Sequential";
    public int? EscalationAfterHours { get; set; }
}

public class SaveApprovalWorkflowDto
{
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<ApprovalStepDto> Steps { get; set; } = new();
}

public class CreateApprovalRequestDto
{
    public int WorkflowId { get; set; }
    public string Reference { get; set; } = string.Empty;
}

public class ApprovalActionDto
{
    public string Decision { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
}
