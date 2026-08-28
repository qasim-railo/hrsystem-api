namespace HRSystem.API.Models;

public class ImportJob : ITenantOwned
{
    public int ImportJobId { get; set; }
    public int TenantId { get; set; }
    public int UserId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Uploaded";
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int ImportedRows { get; set; }
    public int ErrorRows { get; set; }
    public string ErrorsJson { get; set; } = "[]";
    public string RowsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
