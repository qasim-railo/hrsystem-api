namespace HRSystem.API.Models;

public class OnboardingProgress : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Status { get; set; } = "Started";
    public int CompletedStep { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
