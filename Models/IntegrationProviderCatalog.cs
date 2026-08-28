namespace HRSystem.API.Models;

public sealed record IntegrationProviderDefinition(
    string Key,
    string Name,
    string Description,
    string Category,
    IntegrationProviderType ProviderType);

public static class IntegrationProviderCatalog
{
    public static readonly IReadOnlyList<IntegrationProviderDefinition> Providers =
    [
        new("biometric", "Biometric Devices", "Fingerprint and face attendance hardware connectors.", "Access Control", IntegrationProviderType.Biometric),
        new("accounting", "Accounting", "ERP and accounting sync for journal and financial data.", "Finance", IntegrationProviderType.Accounting),
        new("banking", "Banking", "Bank transfer and exchange-house payment integrations.", "Payroll", IntegrationProviderType.Banking),
        new("email", "Email", "Transactional mail delivery and notifications via SMTP or provider APIs.", "Communication", IntegrationProviderType.Email),
        new("sms", "SMS", "Delivery notifications and payroll/attendance alerts.", "Communication", IntegrationProviderType.Sms),
        new("microsoft365", "Microsoft 365", "Exchange, Teams, and identity integration points.", "Collaboration", IntegrationProviderType.Microsoft365),
        new("erp", "ERP", "Back-office operational integrations for HR and business workflows.", "Operations", IntegrationProviderType.Erp)
    ];

    public static IntegrationProviderDefinition? Find(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;
        return Providers.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
    }
}
