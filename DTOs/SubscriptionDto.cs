using HRSystem.API.Models;

namespace HRSystem.API.DTOs;

public class SubscriptionDto
{
    public int SubscriptionId { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? RenewalDate { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class ActivateSubscriptionDto
{
    public int? PlanId { get; set; }
    public string? BillingCycle { get; set; }
    public DateTime? RenewalDate { get; set; }
    public string? Notes { get; set; }
}

public class ChangeSubscriptionPlanDto
{
    public int PlanId { get; set; }
    public string? Notes { get; set; }
}

public class ExtendSubscriptionDto
{
    public DateTime RenewalDate { get; set; }
    public string? Notes { get; set; }
}

public class SubscriptionCheckResult
{
    public bool Allowed { get; init; }
    public SubscriptionStatus? Status { get; init; }
    public string Reason { get; init; } = string.Empty;
}
