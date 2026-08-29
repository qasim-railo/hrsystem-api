namespace HRSystem.API.DTOs;

public class CreateBillingInvoiceDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? DueDate { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? PlanId { get; set; }
    public string? Notes { get; set; }
}

public class RecordPaymentDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string PaymentMethod { get; set; } = "Manual";
    public string Reference { get; set; } = string.Empty;
    public bool ApplyToSubscription { get; set; } = true;
    public string? Notes { get; set; }
}

public class UpdateBillingInvoiceStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class BillingPaymentDto
{
    public int SubscriptionPaymentId { get; set; }
    public int BillingInvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

public class BillingInvoiceDto
{
    public int BillingInvoiceId { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public List<BillingPaymentDto> Payments { get; set; } = new();
}

public class BillingHistoryDto
{
    public List<BillingInvoiceDto> Invoices { get; set; } = new();
    public List<BillingPaymentDto> Payments { get; set; } = new();
}
