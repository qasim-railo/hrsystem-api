namespace HRSystem.API.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    public string EventCode { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? RecipientEmail { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Channel { get; set; } = "InApp";
}
