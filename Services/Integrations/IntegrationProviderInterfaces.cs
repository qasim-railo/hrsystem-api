namespace HRSystem.API.Services.Integrations;

public interface IIntegrationProviderService
{
    string ProviderKey { get; }
    string DisplayName { get; }
    bool RequiresSecretReference { get; }
    bool ValidateConfiguration(string? secretReference, string? baseUrl, string? configurationJson);
}

public interface IBiometricIntegrationService : IIntegrationProviderService
{
}

public interface IAccountingIntegrationService : IIntegrationProviderService
{
}

public interface IBankingIntegrationService : IIntegrationProviderService
{
}

public interface IEmailIntegrationService : IIntegrationProviderService
{
}

public interface ISmsIntegrationService : IIntegrationProviderService
{
}

public interface IMicrosoft365IntegrationService : IIntegrationProviderService
{
}

public interface IErpIntegrationService : IIntegrationProviderService
{
}

public abstract class BaseIntegrationProviderService : IIntegrationProviderService
{
    protected BaseIntegrationProviderService(string providerKey, string displayName, bool requiresSecretReference)
    {
        ProviderKey = providerKey;
        DisplayName = displayName;
        RequiresSecretReference = requiresSecretReference;
    }

    public string ProviderKey { get; }
    public string DisplayName { get; }
    public bool RequiresSecretReference { get; }

    public virtual bool ValidateConfiguration(string? secretReference, string? baseUrl, string? configurationJson)
    {
        if (RequiresSecretReference && string.IsNullOrWhiteSpace(secretReference))
        {
            return false;
        }

        return true;
    }
}

public sealed class BiometricIntegrationService : BaseIntegrationProviderService, IBiometricIntegrationService
{
    public BiometricIntegrationService() : base("biometric", "Biometric Devices", true) { }
}

public sealed class AccountingIntegrationService : BaseIntegrationProviderService, IAccountingIntegrationService
{
    public AccountingIntegrationService() : base("accounting", "Accounting", true) { }
}

public sealed class BankingIntegrationService : BaseIntegrationProviderService, IBankingIntegrationService
{
    public BankingIntegrationService() : base("banking", "Banking", true) { }
}

public sealed class EmailIntegrationService : BaseIntegrationProviderService, IEmailIntegrationService
{
    public EmailIntegrationService() : base("email", "Email", true) { }
}

public sealed class SmsIntegrationService : BaseIntegrationProviderService, ISmsIntegrationService
{
    public SmsIntegrationService() : base("sms", "SMS", true) { }
}

public sealed class Microsoft365IntegrationService : BaseIntegrationProviderService, IMicrosoft365IntegrationService
{
    public Microsoft365IntegrationService() : base("microsoft365", "Microsoft 365", true) { }
}

public sealed class ErpIntegrationService : BaseIntegrationProviderService, IErpIntegrationService
{
    public ErpIntegrationService() : base("erp", "ERP", true) { }
}
