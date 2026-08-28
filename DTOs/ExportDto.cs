namespace HRSystem.API.DTOs;

public sealed record ExportOptionDto(string Code, string Name, string Description, string Permission, bool Sensitive, bool Available, string Endpoint);
