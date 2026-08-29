namespace HRSystem.API.Tenancy;

public sealed class CurrentTenant : ICurrentTenant
{
    public int? TenantId { get; private set; }
    public bool IsPlatformAdmin { get; private set; }
    public bool HasTenantContext => TenantId.HasValue && !IsPlatformAdmin;

    public void SetTenant(int tenantId)
    {
        if (tenantId <= 0)
            throw new ArgumentOutOfRangeException(nameof(tenantId));

        TenantId = tenantId;
        IsPlatformAdmin = false;
    }

    public void SetPlatformAdmin()
    {
        TenantId = null;
        IsPlatformAdmin = true;
    }

    public void Clear()
    {
        TenantId = null;
        IsPlatformAdmin = false;
    }
}
