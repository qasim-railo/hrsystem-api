namespace HRSystem.API.DTOs;

public class OvertimePolicyAssignmentDto
{
    public int Id { get; set; }
    public int OvertimePolicyId { get; set; }
    public string Scope { get; set; } = "All";
    public int? TargetId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveOvertimePolicyAssignmentDto : OvertimePolicyAssignmentDto { }
