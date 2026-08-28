namespace HRSystem.API.DTOs;

public sealed record AuditLogDto(
    int AuditLogId,
    string Action,
    string Entity,
    string EntityId,
    string UserId,
    DateTime CreatedAt,
    string Details);
