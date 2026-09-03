using HRSystem.API.Tenancy;

namespace HRSystem.API.Models;

public class TenantCurrency : ITenantOwned
{
    public int TenantCurrencyId { get; set; }
    public int TenantId { get; set; }
    public int CurrencyId { get; set; }
    public Currency Currency { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
}
