namespace HRSystem.API.Models;

public class ApprovalWorkflow : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
}
