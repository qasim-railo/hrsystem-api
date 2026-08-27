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

    public async Task<SubscriptionCheckResult> CheckAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var subscription = await _db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription == null)
            return new SubscriptionCheckResult { Allowed = false, Reason = "No subscription exists for this tenant." };

        var effectiveStatus = subscription.Status;
        if (subscription.TrialEndDate is DateTime trialEnd && trialEnd <= DateTime.UtcNow &&
            subscription.Status == SubscriptionStatus.Trial)
            effectiveStatus = SubscriptionStatus.Expired;
        else if (subscription.RenewalDate is DateTime renewal && renewal <= DateTime.UtcNow &&
                 subscription.Status == SubscriptionStatus.Active)
            effectiveStatus = SubscriptionStatus.Expired;
        var allowed = effectiveStatus is SubscriptionStatus.Trial or SubscriptionStatus.Active;
        return new SubscriptionCheckResult
        {
            Allowed = allowed,
            Status = effectiveStatus,
            Reason = allowed ? "Subscription is active." : $"Subscription is {effectiveStatus}."
        };
    }
}
