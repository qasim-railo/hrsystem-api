namespace HRSystem.API.DTOs;

public class TenantProfileDto
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public string? CountryCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? TimeZoneId { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
}

public class TenantSettingDto
{
    public string? Value { get; set; }
}
