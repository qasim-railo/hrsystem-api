namespace HRSystem.API.DTOs;

public class FileUploadDto
{
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public IFormFile File { get; set; } = default!;
}

public class FileReplaceDto
{
    public IFormFile File { get; set; } = default!;
}

public class FileRecordDto
{
    public int FileId { get; set; }
    public int TenantId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Extension { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Version { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public class FileSearchRequest
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public string? DocumentType { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
