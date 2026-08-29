namespace HRSystem.API.Tenancy;

public interface ICurrentTenant
{
    int? TenantId { get; }
    bool IsPlatformAdmin { get; }
    bool HasTenantContext { get; }
    void SetTenant(int tenantId);
    void SetPlatformAdmin();
    void Clear();
}
