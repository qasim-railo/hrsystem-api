namespace HRSystem.API.Models;

public class AttendanceConfiguration : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string AllowedSources { get; set; } = "Manual,Excel";
    public int GraceInMinutes { get; set; } = 15;
    public int GraceOutMinutes { get; set; } = 15;
    public string MissingPunchPolicy { get; set; } = "Flag";
    public string LateEarlyRule { get; set; } = "Track";
    public bool ApprovalRequired { get; set; }
    public decimal DefaultWorkingHours { get; set; } = 8;
    public int ExpectedWorkMinutes { get; set; } = 480;
}
