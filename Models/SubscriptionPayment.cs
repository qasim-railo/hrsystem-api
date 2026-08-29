namespace HRSystem.API.Models;

public class SubscriptionPayment : ITenantOwned
{
    public int SubscriptionPaymentId { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public int BillingInvoiceId { get; set; }
    public BillingInvoice BillingInvoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = "Manual";
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = "Paid";
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
