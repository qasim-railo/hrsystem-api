using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Models.Auth;
using HRSystem.API.Services;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AuthService _auth;
    private readonly CurrentTenant _currentTenant;

    public RegistrationController(AppDbContext db, AuthService auth, CurrentTenant currentTenant)
    {
        _db = db;
        _auth = auth;
        _currentTenant = currentTenant;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<RegistrationResultDto>> Register(RegistrationDto dto)
    {
        var registrationNumber = dto.CommercialRegistrationNumber.Trim();
        var administratorEmail = dto.AdministratorEmail.Trim().ToLowerInvariant();
        var countryCode = dto.Country.Trim().ToUpperInvariant();
        var country = await _db.Countries.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Code == countryCode && item.IsActive);
        if (country == null)
            return BadRequest("Select an active PeopleOS country.");
        if (await _db.Companies.IgnoreQueryFilters().AnyAsync(c => c.CommercialRegistrationNumber == registrationNumber))
            return Conflict("A company with this commercial registration number is already registered.");
        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Username.ToLower() == administratorEmail))
            return Conflict("This administrator email is already registered.");

        var code = BuildTenantCode(registrationNumber);
        if (await _db.Tenants.AnyAsync(t => t.Code == code))
            return Conflict("This company registration already has a tenant.");

        var currency = await _db.Currencies.AsNoTracking().SingleAsync(item => item.Code == (country.Code == "QA" ? "QAR" : "USD"));
        var timeZone = await _db.TimeZones.AsNoTracking()
            .OrderBy(item => item.DisplayName)
            .FirstOrDefaultAsync(item => item.CountryCode == country.Code && item.IsActive);
        if (timeZone == null)
            return BadRequest("The selected PeopleOS country does not have an active time zone configured.");
        await using var transaction = await _db.Database.BeginTransactionAsync();
        var tenant = new Tenant
        {
            Name = dto.TradeName?.Trim() ?? dto.LegalName.Trim(),
            Code = code,
            Status = "Trial",
            LifecycleStatus = "Onboarding",
            Country = country.Code,
            Currency = country.Code == "QA" ? "QAR" : "USD",
            TimeZone = timeZone.TimeZoneId,
            CountryCode = country.Code,
            CurrencyCode = country.Code == "QA" ? "QAR" : "USD",
            TimeZoneId = timeZone.TimeZoneId,
            DefaultCountryId = country.CountryId,
            DefaultCurrencyId = currency.CurrencyId,
            DefaultTimeZoneId = timeZone.TimeZoneId,
            TrialStartDate = DateTime.UtcNow,
            TrialEndDate = DateTime.UtcNow.AddDays(14),
            PlanId = 1,
            PlanName = "PeopleOS Essential"
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
        _db.TenantCurrencies.Add(new TenantCurrency { TenantId = tenant.TenantId, CurrencyId = tenant.DefaultCurrencyId });
        _db.EmployeeCategories.AddRange(
            new EmployeeCategory { TenantId = tenant.TenantId, Name = "Labor", Code = "LABOR", SortOrder = 10 },
            new EmployeeCategory { TenantId = tenant.TenantId, Name = "Staff", Code = "STAFF", SortOrder = 20 },
            new EmployeeCategory { TenantId = tenant.TenantId, Name = "Executive Staff", Code = "EXECUTIVE_STAFF", SortOrder = 30 },
            new EmployeeCategory { TenantId = tenant.TenantId, Name = "Managerial", Code = "MANAGERIAL", SortOrder = 40 });
        _db.Subscriptions.Add(new Subscription
        {
            TenantId = tenant.TenantId,
            PlanId = tenant.PlanId,
            Status = SubscriptionStatus.Trial,
            StartDate = tenant.TrialStartDate!.Value,
            RenewalDate = tenant.TrialEndDate,
            TrialStartDate = tenant.TrialStartDate,
            TrialEndDate = tenant.TrialEndDate,
            BillingCycle = "Monthly",
            Notes = "Initial 14-day trial."
        });
        await _db.SaveChangesAsync();

        _currentTenant.SetTenant(tenant.TenantId);
        _db.Companies.Add(new Company
        {
            TenantId = tenant.TenantId,
            Name = tenant.Name,
            LegalName = dto.LegalName.Trim(),
            TradeName = dto.TradeName?.Trim(),
            CommercialRegistrationNumber = registrationNumber,
            Industry = dto.Industry?.Trim(),
            EmployeeCount = 1,
            Address = "To be completed during onboarding",
            Country = tenant.Country,
            Phone = dto.Phone.Trim(),
            Email = dto.AdministratorEmail.Trim(),
            ContactPerson = dto.AdministratorName.Trim(),
            ContactPhone = dto.Phone.Trim()
        });

        var adminRole = await _db.Roles.Include(r => r.RolePermissions)
            .SingleOrDefaultAsync(r => r.Name == "Admin" && r.TenantId == tenant.TenantId);
        if (adminRole == null)
        {
            var template = await _db.Roles.IgnoreQueryFilters().AsNoTracking().Include(r => r.RolePermissions)
                .OrderBy(r => r.Id).FirstOrDefaultAsync(r => r.Name == "Admin");
            var permissionIds = template?.RolePermissions.Select(x => x.PermissionId).ToList()
                ?? await _db.Permissions
                    .Where(permission => permission.Name != "Platform.Tenants")
                    .Select(permission => permission.Id)
                    .ToListAsync();

            if (permissionIds.Count == 0)
                return Problem("No tenant administrator permissions are configured.", statusCode: StatusCodes.Status500InternalServerError);

            adminRole = new Role
            {
                TenantId = tenant.TenantId,
                Name = "Admin",
                RolePermissions = permissionIds.Select(permissionId => new RolePermission { PermissionId = permissionId }).ToList()
            };
            _db.Roles.Add(adminRole);
            await _db.SaveChangesAsync();
        }
        _db.Users.Add(new AppUser
        {
            TenantId = tenant.TenantId,
            Username = administratorEmail,
            PasswordHash = _auth.HashPassword(dto.AdministratorPassword),
            UserRoles = new List<UserRole> { new() { RoleId = adminRole.Id } }
        });

        _db.TenantSettings.AddRange(
            new TenantSetting { TenantId = tenant.TenantId, Key = "WorkingWeek", Value = "Sunday,Monday,Tuesday,Wednesday,Thursday" },
            new TenantSetting { TenantId = tenant.TenantId, Key = "DefaultTimeZone", Value = tenant.TimeZone },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorName", Value = dto.AdministratorName.Trim() },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorEmail", Value = dto.AdministratorEmail.Trim() },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorPhone", Value = dto.Phone.Trim() });
        _db.TenantLeaveTypes.AddRange(
            new TenantLeaveType { TenantId = tenant.TenantId, Name = "Annual", DefaultDays = 21 },
            new TenantLeaveType { TenantId = tenant.TenantId, Name = "Sick", DefaultDays = 14 },
            new TenantLeaveType { TenantId = tenant.TenantId, Name = "Emergency", DefaultDays = 3 });
        _db.OnboardingProgress.Add(new OnboardingProgress { TenantId = tenant.TenantId, Status = "Started", CompletedStep = 1 });
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Created("api/registration", new RegistrationResultDto
        {
            TenantCode = tenant.Code,
            AdministratorEmail = administratorEmail,
            Status = "Started",
            CompletedStep = 1
        });
    }

    [HttpGet("progress")]
    [Authorize]
    public async Task<ActionResult<OnboardingProgressDto>> GetProgress()
    {
        var progress = await _db.OnboardingProgress.AsNoTracking().SingleOrDefaultAsync();
        return progress == null
            ? NotFound()
            : Ok(new OnboardingProgressDto { Status = progress.Status, CompletedStep = progress.CompletedStep, UpdatedAt = progress.UpdatedAt });
    }

    [HttpPut("progress")]
    [Authorize]
    public async Task<ActionResult<OnboardingProgressDto>> UpdateProgress([FromBody] OnboardingProgressDto dto)
    {
        const int maxWizardStep = 8;
        if (dto.CompletedStep < 0 || dto.CompletedStep > maxWizardStep)
            return BadRequest("CompletedStep must be between 0 and 8.");
        var progress = await _db.OnboardingProgress.SingleOrDefaultAsync();
        if (progress == null) return NotFound();
        progress.CompletedStep = dto.CompletedStep;
        progress.Status = dto.CompletedStep >= maxWizardStep ? "Completed" : "InProgress";
        progress.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new OnboardingProgressDto { Status = progress.Status, CompletedStep = progress.CompletedStep, UpdatedAt = progress.UpdatedAt });
    }

    private static string BuildTenantCode(string registrationNumber)
    {
        var normalized = new string(registrationNumber.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        return $"TEN-{normalized}";
    }
}
