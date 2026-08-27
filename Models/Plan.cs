namespace HRSystem.API.Models;

public class Plan
{
    public int PlanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MaxEmployees { get; set; }
    public int MaxUsers { get; set; }
    public int MaxBranches { get; set; }
    public long MaxStorageBytes { get; set; }
    public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
