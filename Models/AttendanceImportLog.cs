namespace HRSystem.API.Models;

public class AttendanceImportLog : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "Excel";
    public string FileName { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int ErrorRows { get; set; }
    public string Errors { get; set; } = string.Empty;
}
