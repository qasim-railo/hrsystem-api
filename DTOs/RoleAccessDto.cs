namespace HRSystem.API.DTOs;

public class RoleAccessDto
{
    public string Name { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public List<PermissionScopeDto> PermissionScopes { get; set; } = new();
}

public class PermissionScopeDto
{
    public string Permission { get; set; } = string.Empty;
    public string DataScope { get; set; } = "TenantWide";
    public List<int> ScopeIds { get; set; } = new();
}
