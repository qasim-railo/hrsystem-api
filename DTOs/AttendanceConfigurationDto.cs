namespace HRSystem.API.DTOs;

public class AttendanceConfigurationDto
{
    public string AllowedSources { get; set; } = "Manual,Excel";
    public int GraceInMinutes { get; set; } = 15;
    public int GraceOutMinutes { get; set; } = 15;
    public string MissingPunchPolicy { get; set; } = "Flag";
    public string LateEarlyRule { get; set; } = "Track";
    public bool ApprovalRequired { get; set; }
    public decimal DefaultWorkingHours { get; set; } = 8;
    public int ExpectedWorkMinutes { get; set; } = 480;
    public List<TenantWorkingDayDto> WorkingDays { get; set; } = new();
}

public class TenantWorkingDayDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsWorkingDay { get; set; }
    public TimeSpan DefaultStartTime { get; set; } = new(8, 0, 0);
    public TimeSpan DefaultEndTime { get; set; } = new(17, 0, 0);
    public int BreakMinutes { get; set; } = 60;
}

public class AttendanceImportLogDto
{
    public int Id { get; set; }
    public DateTime ImportedAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int ErrorRows { get; set; }
    public string Errors { get; set; } = string.Empty;
}
