namespace HRSystem.API.Models;

public class Position : ITenantOwned
{
    public int PositionId { get; set; }
    public int TenantId { get; set; }
    public int? TeamId { get; set; }
    public Team? Team { get; set; }
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? EmployeeCategoryId { get; set; }
    public EmployeeCategory? EmployeeCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime? ArchivedAt { get; set; }
}
