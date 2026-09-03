using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs;

public class CountryMasterDto
{
    public int CountryId { get; set; }
    [Required, StringLength(2, MinimumLength = 2)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class CurrencyMasterDto
{
    public int CurrencyId { get; set; }
    [Required, StringLength(3, MinimumLength = 3)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(10)] public string Symbol { get; set; } = string.Empty;
    [Range(0, 6)] public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;
}

public class TimeZoneMasterDto
{
    [Required, StringLength(100)] public string TimeZoneId { get; set; } = string.Empty;
    [Required, StringLength(150)] public string DisplayName { get; set; } = string.Empty;
    [StringLength(2, MinimumLength = 2)] public string? CountryCode { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TenantCurrenciesDto
{
    public List<int> CurrencyIds { get; set; } = new();
}
