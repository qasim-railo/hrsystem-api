namespace HRSystem.API.Models;

public class Tenant
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? SubscriptionReference { get; set; }
    public string Country { get; set; } = "QA";
    public string Currency { get; set; } = "QAR";
    public string TimeZone { get; set; } = "Asia/Qatar";
    public string CountryCode { get; set; } = "QA";
    public string CurrencyCode { get; set; } = "QAR";
    public string TimeZoneId { get; set; } = "Asia/Qatar";
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string NumberFormat { get; set; } = "en-QA";
    public string DefaultLanguage { get; set; } = "en";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string LifecycleStatus { get; set; } = "Active";
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string PlanName { get; set; } = "PeopleOS Essential";
    public int PlanId { get; set; } = 1;
    public Plan Plan { get; set; } = null!;
    public long StorageUsedBytes { get; set; }
    public string BillingStatus { get; set; } = "Not configured";
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
