namespace HRSystem.API.DTOs;

public class TenantProfileDto
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
}

public class TenantSettingDto
{
    public string? Value { get; set; }
}
