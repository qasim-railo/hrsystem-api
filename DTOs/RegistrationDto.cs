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
    [Range(1, int.MaxValue)]
    public int EmployeeCount { get; set; }
    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;
    [Required, StringLength(2, MinimumLength = 2)]
    public string Country { get; set; } = "QA";
    [Required, Phone, StringLength(40)]
    public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;
    [Url, StringLength(300)]
    public string? Website { get; set; }
    [Required, StringLength(150)]
    public string ContactPerson { get; set; } = string.Empty;
    [Required, Phone, StringLength(40)]
    public string ContactPhone { get; set; } = string.Empty;
    [Required, StringLength(100)]
    public string AdministratorUsername { get; set; } = string.Empty;
    [Required, StringLength(200, MinimumLength = 6)]
    public string AdministratorPassword { get; set; } = string.Empty;
    [Required, StringLength(150)]
    public string AdministratorName { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(200)]
    public string AdministratorEmail { get; set; } = string.Empty;
    [Required, Phone, StringLength(40)]
    public string AdministratorPhone { get; set; } = string.Empty;
}

public class RegistrationResultDto
{
    public string TenantCode { get; set; } = string.Empty;
    public string AdministratorUsername { get; set; } = string.Empty;
    public string Status { get; set; } = "Started";
    public int CompletedStep { get; set; }
}
