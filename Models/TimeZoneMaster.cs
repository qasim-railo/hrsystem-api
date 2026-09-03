namespace HRSystem.API.Models;

public class TimeZoneMaster
{
    public string TimeZoneId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? CountryCode { get; set; }
    public bool IsActive { get; set; } = true;
}
