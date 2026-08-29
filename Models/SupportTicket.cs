namespace HRSystem.API.Models;

public class SupportArticle : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Category { get; set; } = "General";
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class SupportTicket : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public int? RequesterUserId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<SupportTicketMessage> Messages { get; set; } = new List<SupportTicketMessage>();
    public ICollection<SupportTicketAttachment> Attachments { get; set; } = new List<SupportTicketAttachment>();
}

public class SupportTicketMessage : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }
    public int? SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = "User";
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SupportTicketAttachment> Attachments { get; set; } = new List<SupportTicketAttachment>();
}

public class SupportTicketAttachment : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }
    public int? MessageId { get; set; }
    public SupportTicketMessage? Message { get; set; }
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public int? UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
