namespace HRSystem.API.Models;

public class Team : ITenantOwned
{
    public int TeamId { get; set; }
    public int TenantId { get; set; }
    public int SectionId { get; set; }
    public Section Section { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public ICollection<Position> Positions { get; set; } = new List<Position>();
}
