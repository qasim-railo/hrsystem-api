namespace HRSystem.API.DTOs;

public class IntegrationConnectionDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string SecretReference { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? ConfigurationJson { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class IntegrationUpdateDto
{
    public bool IsEnabled { get; set; }
    public string? SecretReference { get; set; }
    public string? BaseUrl { get; set; }
    public string? ConfigurationJson { get; set; }
}
