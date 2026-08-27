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
        var username = dto.AdministratorUsername.Trim();
        if (await _db.Companies.IgnoreQueryFilters().AnyAsync(c => c.CommercialRegistrationNumber == registrationNumber))
            return Conflict("A company with this commercial registration number is already registered.");
        if (await _db.Users.AnyAsync(u => u.Username == username))
            return Conflict("This administrator username is already registered.");

        var code = BuildTenantCode(registrationNumber);
        if (await _db.Tenants.AnyAsync(t => t.Code == code))
            return Conflict("This company registration already has a tenant.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        var tenant = new Tenant
        {
            Name = dto.TradeName?.Trim() ?? dto.LegalName.Trim(),
            Code = code,
            Status = "Trial",
            LifecycleStatus = "Onboarding",
            Country = dto.Country.Trim().ToUpperInvariant(),
            Currency = dto.Country.Equals("QA", StringComparison.OrdinalIgnoreCase) ? "QAR" : "USD",
            TimeZone = dto.Country.Equals("QA", StringComparison.OrdinalIgnoreCase) ? "Asia/Qatar" : "UTC",
            TrialStartDate = DateTime.UtcNow,
            TrialEndDate = DateTime.UtcNow.AddDays(14),
            PlanId = 1,
            PlanName = "PeopleOS Essential"
        };
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();
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
            EmployeeCount = dto.EmployeeCount,
            Address = dto.Address.Trim(),
            Country = tenant.Country,
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            Website = dto.Website?.Trim(),
            ContactPerson = dto.ContactPerson.Trim(),
            ContactPhone = dto.ContactPhone.Trim()
        });

        var adminRole = await _db.Roles.Include(r => r.RolePermissions)
            .SingleOrDefaultAsync(r => r.Name == "Admin" && r.TenantId == tenant.TenantId);
        if (adminRole == null)
        {
            var template = await _db.Roles.AsNoTracking().Include(r => r.RolePermissions)
                .OrderBy(r => r.Id).FirstAsync(r => r.Name == "Admin");
            adminRole = new Role
            {
                TenantId = tenant.TenantId,
                Name = "Admin",
                RolePermissions = template.RolePermissions.Select(x => new RolePermission { PermissionId = x.PermissionId }).ToList()
            };
            _db.Roles.Add(adminRole);
            await _db.SaveChangesAsync();
        }
        _db.Users.Add(new AppUser
        {
            TenantId = tenant.TenantId,
            Username = username,
            PasswordHash = _auth.HashPassword(dto.AdministratorPassword),
            UserRoles = new List<UserRole> { new() { RoleId = adminRole.Id } }
        });

        _db.TenantSettings.AddRange(
            new TenantSetting { TenantId = tenant.TenantId, Key = "WorkingWeek", Value = "Sunday,Monday,Tuesday,Wednesday,Thursday" },
            new TenantSetting { TenantId = tenant.TenantId, Key = "DefaultTimeZone", Value = tenant.TimeZone },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorName", Value = dto.AdministratorName.Trim() },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorEmail", Value = dto.AdministratorEmail.Trim() },
            new TenantSetting { TenantId = tenant.TenantId, Key = "AdministratorPhone", Value = dto.AdministratorPhone.Trim() });
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
            AdministratorUsername = username,
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
        if (dto.CompletedStep < 0 || dto.CompletedStep > 6)
            return BadRequest("CompletedStep must be between 0 and 6.");
        var progress = await _db.OnboardingProgress.SingleOrDefaultAsync();
        if (progress == null) return NotFound();
        progress.CompletedStep = dto.CompletedStep;
        progress.Status = dto.CompletedStep >= 6 ? "Completed" : "InProgress";
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
