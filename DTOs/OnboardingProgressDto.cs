namespace HRSystem.API.DTOs;

public class OnboardingProgressDto
{
    public string Status { get; set; } = string.Empty;
    public int CompletedStep { get; set; }
    public DateTime UpdatedAt { get; set; }
}
