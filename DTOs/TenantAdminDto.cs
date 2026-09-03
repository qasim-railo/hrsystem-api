namespace HRSystem.API.DTOs;

public class TenantProfileDto
{
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? Currency { get; set; }
    public string? TimeZone { get; set; }
    public int? DefaultCountryId { get; set; }
    public int? DefaultCurrencyId { get; set; }
    public string? DefaultTimeZoneId { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
}

public class TenantSettingDto
{
    public string? Value { get; set; }
}

public class TenantBrandingDto
{
    public string? DisplayName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? CompanyLogoUrl { get; set; }
    public string? PayslipLogoUrl { get; set; }
    public string? ReportHeader { get; set; }
    public string? EmailFooter { get; set; }
}

public class TenantSetupStepDto
{
    public int StepNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public bool IsSkippable { get; set; }
}

public class TenantSetupProgressDto
{
    public int CompletedStep { get; set; }
    public int TotalSteps { get; set; }
    public int PercentComplete { get; set; }
    public string Status { get; set; } = "Started";
    public DateTime UpdatedAt { get; set; }
    public List<TenantSetupStepDto> Steps { get; set; } = new();
}

public class TenantSetupProgressUpdateDto
{
    public int CompletedStep { get; set; }
}
