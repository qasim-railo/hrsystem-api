namespace HRSystem.API.Tenancy;

public interface ICurrentTenant
{
    int? TenantId { get; }
    void SetTenant(int tenantId);
    void Clear();
}
