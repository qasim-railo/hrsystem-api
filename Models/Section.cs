namespace HRSystem.API.Models;

public class Section : ITenantOwned
{
    public int SectionId { get; set; }
    public int TenantId { get; set; }
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}
