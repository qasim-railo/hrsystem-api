using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class SubscriptionCheckService : ISubscriptionCheckService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _currentTenant;

    public SubscriptionCheckService(AppDbContext db, ICurrentTenant currentTenant)
    {
        _db = db;
        _currentTenant = currentTenant;
    }

    public Task<SubscriptionCheckResult> CheckCurrentTenantAsync(CancellationToken cancellationToken = default)
        => _currentTenant.TenantId is int tenantId
            ? CheckAsync(tenantId, cancellationToken)
            : Task.FromResult(new SubscriptionCheckResult { Allowed = false, Reason = "No tenant context is available." });

    public Task<FeatureCheckResult> CheckCurrentTenantFeatureAsync(string featureCode, CancellationToken cancellationToken = default)
        => _currentTenant.TenantId is int tenantId
            ? CheckFeatureAsync(tenantId, featureCode, cancellationToken)
            : Task.FromResult(new FeatureCheckResult { Allowed = false, Reason = "No tenant context is available.", FeatureCode = featureCode?.Trim() ?? string.Empty });

    public async Task<SubscriptionCheckResult> CheckAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription == null)
            return new SubscriptionCheckResult { Allowed = false, Reason = "No subscription exists for this tenant." };

        var effectiveStatus = GetEffectiveStatus(subscription);
        var allowed = effectiveStatus is SubscriptionStatus.Trial or SubscriptionStatus.Active;
        return new SubscriptionCheckResult
        {
            Allowed = allowed,
            Status = effectiveStatus,
            TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(subscription.TrialEndDate, subscription.Status),
            UpgradeRequired = false,
            Reason = allowed ? "Subscription is active." : $"Subscription is {effectiveStatus}."
        };
    }

    public async Task<FeatureCheckResult> CheckFeatureAsync(int tenantId, string featureCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(featureCode))
            return new FeatureCheckResult { Allowed = false, Reason = "A feature code is required." };

        var code = featureCode.Trim();
        var subscription = await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .Include(s => s.Plan)
            .ThenInclude(p => p.Features)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
            return new FeatureCheckResult { Allowed = false, FeatureCode = code, Reason = "No subscription exists for this tenant." };

        var effectiveStatus = GetEffectiveStatus(subscription);
        if (effectiveStatus is not SubscriptionStatus.Trial and not SubscriptionStatus.Active)
        {
            return new FeatureCheckResult
            {
                Allowed = false,
                Status = effectiveStatus,
                TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(subscription.TrialEndDate, subscription.Status),
                FeatureCode = code,
                CurrentPlanCode = subscription.Plan?.Code ?? string.Empty,
                CurrentPlanName = subscription.Plan?.Name ?? string.Empty,
                Reason = $"Subscription is {effectiveStatus}. Activate or renew the tenant subscription before accessing {code}."
            };
        }

        var normalizedCode = code.ToUpperInvariant();
        var availableFeatures = subscription.Plan?.Features
            .Where(f => f.IsEnabled)
            .Select(f => f.FeatureCode)
            .ToList() ?? new List<string>();

        var allowed = availableFeatures.Contains(normalizedCode, StringComparer.OrdinalIgnoreCase);
        if (!allowed)
        {
            return new FeatureCheckResult
            {
                Allowed = false,
                Status = effectiveStatus,
                TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(subscription.TrialEndDate, subscription.Status),
                UpgradeRequired = true,
                FeatureCode = normalizedCode,
                CurrentPlanCode = subscription.Plan?.Code ?? string.Empty,
                CurrentPlanName = subscription.Plan?.Name ?? string.Empty,
                AvailableFeatures = availableFeatures,
                Reason = $"Feature '{normalizedCode}' is not included in the current {subscription.Plan?.Name ?? "plan"}. Upgrade required."
            };
        }

        return new FeatureCheckResult
        {
            Allowed = true,
            Status = effectiveStatus,
            TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(subscription.TrialEndDate, subscription.Status),
            UpgradeRequired = false,
            FeatureCode = normalizedCode,
            CurrentPlanCode = subscription.Plan?.Code ?? string.Empty,
            CurrentPlanName = subscription.Plan?.Name ?? string.Empty,
            AvailableFeatures = availableFeatures,
            Reason = $"Feature '{normalizedCode}' is enabled on {subscription.Plan?.Name ?? "the current plan"}."
        };
    }

    private static SubscriptionStatus GetEffectiveStatus(Subscription subscription)
    {
        var effectiveStatus = subscription.Status;
        if (subscription.TrialEndDate is DateTime trialEnd && trialEnd <= DateTime.UtcNow && subscription.Status == SubscriptionStatus.Trial)
            effectiveStatus = SubscriptionStatus.Expired;
        else if (subscription.RenewalDate is DateTime renewal && renewal <= DateTime.UtcNow && subscription.Status == SubscriptionStatus.Active)
            effectiveStatus = SubscriptionStatus.Expired;
        return effectiveStatus;
    }
}
