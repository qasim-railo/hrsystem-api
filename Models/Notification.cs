namespace HRSystem.API.Models;

public class NotificationTemplate : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class Notification : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int? UserId { get; set; }
    public string? RecipientEmail { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
