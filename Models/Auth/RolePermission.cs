namespace HRSystem.API.Models.Auth;

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
    public string DataScope { get; set; } = "TenantWide";
    public string ScopeIdsJson { get; set; } = "[]";
}
