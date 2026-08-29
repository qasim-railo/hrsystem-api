using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/tenant")]
public class TenantBrandingController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;

    public TenantBrandingController(AppDbContext db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    [HttpGet("branding")]
    [Authorize]
    public async Task<IActionResult> GetBranding()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();

        return Ok(new TenantBrandingDto
        {
            DisplayName = string.IsNullOrWhiteSpace(tenant.DisplayName) ? tenant.Name : tenant.DisplayName,
            PrimaryColor = string.IsNullOrWhiteSpace(tenant.PrimaryColor) ? "#1f5c9c" : tenant.PrimaryColor,
            CompanyLogoUrl = tenant.CompanyLogoUrl,
            PayslipLogoUrl = tenant.PayslipLogoUrl,
            ReportHeader = tenant.ReportHeader,
            EmailFooter = tenant.EmailFooter
        });
    }

    [HttpPut("branding")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> UpdateBranding(TenantBrandingDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();

        var displayName = (dto.DisplayName ?? tenant.DisplayName ?? tenant.Name).Trim();
        if (string.IsNullOrWhiteSpace(displayName)) return BadRequest("Display name is required.");
        if (displayName.Length > 200) return BadRequest("Display name must be 200 characters or fewer.");

        var primaryColor = (dto.PrimaryColor ?? tenant.PrimaryColor ?? "#1f5c9c").Trim();
        if (!string.IsNullOrWhiteSpace(primaryColor) && !IsValidHexColor(primaryColor))
            return BadRequest("Primary color must use a valid hex value such as #1f5c9c.");

        var companyLogoUrl = dto.CompanyLogoUrl?.Trim() ?? tenant.CompanyLogoUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(companyLogoUrl) && !IsValidBrandingUrl(companyLogoUrl))
            return BadRequest("Company logo must be a valid https, http, or data URL.");

        var payslipLogoUrl = dto.PayslipLogoUrl?.Trim() ?? tenant.PayslipLogoUrl ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(payslipLogoUrl) && !IsValidBrandingUrl(payslipLogoUrl))
            return BadRequest("Payslip logo must be a valid https, http, or data URL.");

        tenant.DisplayName = displayName;
        tenant.PrimaryColor = primaryColor;
        tenant.CompanyLogoUrl = companyLogoUrl;
        tenant.PayslipLogoUrl = payslipLogoUrl;
        tenant.ReportHeader = dto.ReportHeader?.Trim() ?? tenant.ReportHeader ?? string.Empty;
        tenant.EmailFooter = dto.EmailFooter?.Trim() ?? tenant.EmailFooter ?? string.Empty;

        if (tenant.ReportHeader.Length > 500) return BadRequest("Report header must be 500 characters or fewer.");
        if (tenant.EmailFooter.Length > 1000) return BadRequest("Email footer must be 1000 characters or fewer.");

        await _db.SaveChangesAsync();
        return Ok(new TenantBrandingDto
        {
            DisplayName = tenant.DisplayName,
            PrimaryColor = tenant.PrimaryColor,
            CompanyLogoUrl = tenant.CompanyLogoUrl,
            PayslipLogoUrl = tenant.PayslipLogoUrl,
            ReportHeader = tenant.ReportHeader,
            EmailFooter = tenant.EmailFooter
        });
    }

    private static bool IsValidHexColor(string value)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$");
    }

    private static bool IsValidBrandingUrl(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == "data";
        return false;
    }
}
