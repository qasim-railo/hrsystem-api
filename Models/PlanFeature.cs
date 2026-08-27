namespace HRSystem.API.Models;

public class PlanFeature
{
    public int PlanFeatureId { get; set; }
    public int PlanId { get; set; }
    public string FeatureCode { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public Plan Plan { get; set; } = null!;
}
