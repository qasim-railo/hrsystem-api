using System.ComponentModel.DataAnnotations;

namespace HRSystem.API.DTOs;

public class RegistrationDto
{
    [Required, StringLength(200)]
    public string LegalName { get; set; } = string.Empty;
    [StringLength(200)]
    public string? TradeName { get; set; }
    [Required, StringLength(50)]
    public string CommercialRegistrationNumber { get; set; } = string.Empty;
    [StringLength(120)]
    public string? Industry { get; set; }
    [Required, StringLength(2, MinimumLength = 2)]
    public string Country { get; set; } = "QA";
    [Required, Phone, StringLength(40)]
    public string Phone { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 6)]
    public string AdministratorPassword { get; set; } = string.Empty;
    [Required, StringLength(150)]
    public string AdministratorName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(200)]
    public string AdministratorEmail { get; set; } = string.Empty;
}

public class RegistrationResultDto
{
    public string TenantCode { get; set; } = string.Empty;
    public string AdministratorEmail { get; set; } = string.Empty;
    public string Status { get; set; } = "Started";
    public int CompletedStep { get; set; }
}
