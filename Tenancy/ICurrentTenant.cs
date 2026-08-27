namespace HRSystem.API.Tenancy;

public interface ICurrentTenant
{
    int? TenantId { get; }
}
