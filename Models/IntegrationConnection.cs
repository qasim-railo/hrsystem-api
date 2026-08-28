namespace HRSystem.API.Models;

public enum IntegrationProviderType
{
    Biometric,
    Accounting,
    Banking,
    Email,
    Sms,
    Microsoft365,
    Erp
}

public class IntegrationConnection : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public IntegrationProviderType ProviderType { get; set; }
    public bool IsEnabled { get; set; }
    public string SecretReference { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string? ConfigurationJson { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
