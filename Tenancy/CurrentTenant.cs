namespace HRSystem.API.Tenancy;

public sealed class CurrentTenant : ICurrentTenant
{
    public int? TenantId { get; private set; }

    public void SetTenant(int tenantId)
    {
        if (tenantId <= 0)
            throw new ArgumentOutOfRangeException(nameof(tenantId));

        TenantId = tenantId;
    }

    public void Clear()
    {
        TenantId = null;
    }
}
