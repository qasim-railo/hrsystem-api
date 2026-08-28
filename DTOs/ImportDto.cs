namespace HRSystem.API.DTOs;
public sealed record ImportJobDto(int ImportJobId, string EntityType, string FileName, string Status, int TotalRows, int ValidRows, int ImportedRows, int ErrorRows, IReadOnlyList<string> Errors);
