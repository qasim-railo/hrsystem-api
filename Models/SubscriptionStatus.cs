using System.Text.Json.Serialization;

namespace HRSystem.API.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SubscriptionStatus
{
    Trial,
    Active,
    PastDue,
    Suspended,
    Cancelled,
    Expired
}
