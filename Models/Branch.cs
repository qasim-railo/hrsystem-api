namespace HRSystem.API.Models;

public class Branch : ITenantOwned
{
    public int BranchId { get; set; }
    public int TenantId { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
