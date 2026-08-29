using System.Text.Json.Serialization;

namespace HRSystem.API.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BillingInvoiceStatus
{
    Draft,
    Open,
    Paid,
    Overdue,
    Cancelled
}
