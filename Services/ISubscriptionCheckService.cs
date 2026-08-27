using HRSystem.API.DTOs;

namespace HRSystem.API.Services;

public interface ISubscriptionCheckService
{
    Task<SubscriptionCheckResult> CheckAsync(int tenantId, CancellationToken cancellationToken = default);
    Task<SubscriptionCheckResult> CheckCurrentTenantAsync(CancellationToken cancellationToken = default);
}
