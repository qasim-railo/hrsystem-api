namespace HRSystem.API.DTOs;

public sealed class SupportHelpCenterDto
{
    public string ContactEmail { get; set; } = "qasim.railo@gmail.com";
    public IReadOnlyList<string> ContactPhones { get; set; } = ["+974 74001784", "+92 3105293728"];
    public string SupportHours { get; set; } = "Sunday to Thursday, 8:00 AM – 6:00 PM";
    public IReadOnlyList<SupportArticleDto> Articles { get; set; } = Array.Empty<SupportArticleDto>();
}

public sealed class SupportArticleDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
}

public sealed class CreateSupportTicketRequest
{
    public string Category { get; set; } = "General";
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string? RequesterName { get; set; }
    public string? RequesterEmail { get; set; }
    public string? Source { get; set; }
    public List<int> AttachmentFileIds { get; set; } = new();
}

public sealed class UpdateSupportTicketStatusRequest
{
    public string Status { get; set; } = "Open";
}

public sealed class CreateSupportTicketMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public List<int> AttachmentFileIds { get; set; } = new();
    public bool IsInternal { get; set; }
}

public sealed class SupportTicketAttachmentDto
{
    public int Id { get; set; }
    public int FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileType { get; set; }
}

public sealed class SupportTicketMessageDto
{
    public int Id { get; set; }
    public int? SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<SupportTicketAttachmentDto> Attachments { get; set; } = Array.Empty<SupportTicketAttachmentDto>();
}

public sealed class SupportTicketDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? RequesterUserId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string? Source { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public IReadOnlyList<SupportTicketMessageDto> Messages { get; set; } = Array.Empty<SupportTicketMessageDto>();
    public IReadOnlyList<SupportTicketAttachmentDto> Attachments { get; set; } = Array.Empty<SupportTicketAttachmentDto>();
}
