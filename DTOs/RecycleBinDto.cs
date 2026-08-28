namespace HRSystem.API.DTOs;

public sealed record RecycleBinItemDto(string EntityType, string EntityId, string DisplayName, DateTime ArchivedAt);
