namespace HRSystem.API.Models;

public interface ITenantOwned
{
    int TenantId { get; set; }
}
