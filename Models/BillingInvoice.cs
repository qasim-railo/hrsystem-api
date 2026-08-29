namespace HRSystem.API.Models;

public class BillingInvoice : ITenantOwned
{
    public int BillingInvoiceId { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public int SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public int? PlanId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public BillingInvoiceStatus Status { get; set; } = BillingInvoiceStatus.Open;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(14);
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public string? Notes { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<SubscriptionPayment> Payments { get; set; } = new List<SubscriptionPayment>();
}
