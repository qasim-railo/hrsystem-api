using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Services;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Authorize(Policy = "Users.Manage")]
[Route("api/tenant")]
public class TenantAdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public TenantAdminController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        return Ok(ProfileResponse(tenant));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(TenantProfileDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.Name)) tenant.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Country)) tenant.Country = dto.Country.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Currency)) tenant.Currency = dto.Currency.Trim();
        if (!string.IsNullOrWhiteSpace(dto.TimeZone)) tenant.TimeZone = dto.TimeZone.Trim();
        if (!string.IsNullOrWhiteSpace(dto.CountryCode)) tenant.CountryCode = dto.CountryCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.CurrencyCode)) tenant.CurrencyCode = dto.CurrencyCode.Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(dto.TimeZoneId)) tenant.TimeZoneId = dto.TimeZoneId.Trim();
        if (!string.IsNullOrWhiteSpace(dto.DateFormat)) tenant.DateFormat = dto.DateFormat.Trim();
        if (!string.IsNullOrWhiteSpace(dto.NumberFormat)) tenant.NumberFormat = dto.NumberFormat.Trim();
        if (!IsValidProfile(tenant))
            return BadRequest("Country, currency, timezone, date format, or number format is invalid.");
        tenant.Country = tenant.CountryCode;
        tenant.Currency = tenant.CurrencyCode;
        tenant.TimeZone = tenant.TimeZoneId;
        await _db.SaveChangesAsync();
        return Ok(ProfileResponse(tenant));
    }

    private static object ProfileResponse(Tenant tenant) => new
    {
        tenant.TenantId, tenant.Name, tenant.Code, tenant.Country, tenant.Currency, tenant.TimeZone,
        tenant.CountryCode, tenant.CurrencyCode, tenant.TimeZoneId, tenant.DateFormat, tenant.NumberFormat,
        tenant.Status, tenant.LifecycleStatus
    };

    private static bool IsValidProfile(Tenant tenant)
    {
        if (tenant.CountryCode.Length != 2 || tenant.CurrencyCode.Length != 3 ||
            string.IsNullOrWhiteSpace(tenant.DateFormat) || string.IsNullOrWhiteSpace(tenant.NumberFormat))
            return false;
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(tenant.TimeZoneId); }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
        return true;
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        return Ok(await _db.TenantSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync());
    }

    [HttpPut("settings/{key}")]
    public async Task<IActionResult> SetSetting(string key, TenantSettingDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        if (string.IsNullOrWhiteSpace(key)) return BadRequest("Setting key is required.");
        var setting = await _db.TenantSettings.SingleOrDefaultAsync(x => x.TenantId == id && x.Key == key);
        if (setting == null) _db.TenantSettings.Add(setting = new TenantSetting { TenantId = id, Key = key.Trim() });
        setting.Value = dto.Value ?? string.Empty;
        await _db.SaveChangesAsync();
        return Ok(setting);
    }

    [HttpGet("settings-center")]
    public async Task<ActionResult<TenantSettingsCenterDto>> SettingsCenter()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var overrides = await _db.TenantSettings.AsNoTracking()
            .Where(x => x.TenantId == id).ToDictionaryAsync(x => x.Key, StringComparer.OrdinalIgnoreCase);
        var sections = TenantSettingsCatalog.Definitions
            .GroupBy(x => new { x.SectionKey, x.SectionName })
            .Select(group => new TenantSettingsSectionDto
            {
                Key = group.Key.SectionKey,
                Name = group.Key.SectionName,
                Settings = group.Select(definition =>
                {
                    var overridden = overrides.TryGetValue(definition.Key, out var setting);
                    return new TenantSettingItemDto
                    {
                        Key = definition.Key,
                        Label = definition.Label,
                        ValueType = definition.ValueType,
                        Value = overridden ? setting!.Value : definition.DefaultValue,
                        DefaultValue = definition.DefaultValue,
                        IsOverridden = overridden,
                        Options = definition.Options
                    };
                }).ToList()
            }).ToList();
        return Ok(new TenantSettingsCenterDto { Sections = sections });
    }

    [HttpPut("settings-center/{key}")]
    public async Task<ActionResult<TenantSettingItemDto>> SetTypedSetting(string key, TenantSettingDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var definition = TenantSettingsCatalog.Find(key);
        if (definition is null) return NotFound("The setting is not supported.");
        var value = dto.Value?.Trim() ?? string.Empty;
        if (!IsValidValue(definition, value))
            return BadRequest($"'{definition.Label}' must be a valid {definition.ValueType} value.");

        var setting = await _db.TenantSettings.SingleOrDefaultAsync(x => x.TenantId == id && x.Key == definition.Key);
        if (setting is null)
            _db.TenantSettings.Add(setting = new TenantSetting { TenantId = id, Key = definition.Key });
        setting.Value = value;
        await _db.SaveChangesAsync();
        return Ok(new TenantSettingItemDto
        {
            Key = definition.Key, Label = definition.Label, ValueType = definition.ValueType,
            Value = value, DefaultValue = definition.DefaultValue, IsOverridden = true, Options = definition.Options
        });
    }

    [HttpGet("integrations")]
    public async Task<ActionResult<object>> Integrations()
    {
        if (_tenant.TenantId is not int id) return Forbid();

        var connections = await _db.IntegrationConnections.AsNoTracking()
            .Where(x => x.TenantId == id)
            .ToDictionaryAsync(x => x.ProviderKey, StringComparer.OrdinalIgnoreCase);

        var providers = IntegrationProviderCatalog.Providers.Select(definition =>
        {
            var connected = connections.TryGetValue(definition.Key, out var connection);
            return new
            {
                key = definition.Key,
                name = definition.Name,
                description = definition.Description,
                category = definition.Category,
                providerType = definition.ProviderType.ToString(),
                isEnabled = connected && connection!.IsEnabled,
                isConfigured = connected && !string.IsNullOrWhiteSpace(connection.SecretReference),
                secretReference = connected ? connection.SecretReference : null,
                baseUrl = connected ? connection.BaseUrl : null,
                configurationJson = connected ? connection.ConfigurationJson : null,
                lastTestedAt = connected ? connection.LastTestedAt : null
            };
        }).ToList();

        return Ok(new { providers, connectedCount = providers.Count(x => x.isConfigured || x.isEnabled) });
    }

    [HttpPut("integrations/{providerKey}")]
    public async Task<ActionResult<IntegrationConnectionDto>> UpdateIntegration(string providerKey, IntegrationUpdateDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();

        var definition = IntegrationProviderCatalog.Find(providerKey);
        if (definition is null) return NotFound("The requested integration provider is not supported.");

        var provider = await _db.IntegrationConnections.SingleOrDefaultAsync(x => x.TenantId == id && x.ProviderKey == definition.Key);
        if (provider is null)
        {
            provider = new IntegrationConnection
            {
                TenantId = id,
                ProviderKey = definition.Key,
                ProviderName = definition.Name,
                ProviderType = definition.ProviderType,
                SecretReference = dto.SecretReference ?? string.Empty,
                BaseUrl = dto.BaseUrl,
                ConfigurationJson = dto.ConfigurationJson,
                IsEnabled = dto.IsEnabled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.IntegrationConnections.Add(provider);
        }
        else
        {
            provider.ProviderName = definition.Name;
            provider.ProviderType = definition.ProviderType;
            provider.IsEnabled = dto.IsEnabled;
            provider.SecretReference = dto.SecretReference ?? provider.SecretReference;
            provider.BaseUrl = dto.BaseUrl ?? provider.BaseUrl;
            provider.ConfigurationJson = dto.ConfigurationJson ?? provider.ConfigurationJson;
            provider.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new IntegrationConnectionDto
        {
            Id = provider.Id,
            TenantId = provider.TenantId,
            ProviderKey = provider.ProviderKey,
            ProviderName = provider.ProviderName,
            Category = definition.Category,
            IsEnabled = provider.IsEnabled,
            SecretReference = provider.SecretReference,
            BaseUrl = provider.BaseUrl,
            ConfigurationJson = provider.ConfigurationJson,
            LastTestedAt = provider.LastTestedAt,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt
        });
    }

    [HttpGet("setup-wizard")]
    public async Task<ActionResult<TenantSetupProgressDto>> GetSetupWizard()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var progress = await _db.OnboardingProgress.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == id);
        return Ok(BuildSetupProgress(progress, id));
    }

    [HttpPut("setup-wizard")]
    public async Task<ActionResult<TenantSetupProgressDto>> UpdateSetupWizard(TenantSetupProgressUpdateDto dto)
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var maxStep = TenantSetupCatalog.StepDefinitions.Count;
        if (dto.CompletedStep < 0 || dto.CompletedStep > maxStep)
            return BadRequest("CompletedStep must be between 0 and the total setup steps.");

        var progress = await _db.OnboardingProgress.SingleOrDefaultAsync(x => x.TenantId == id);
        if (progress is null)
        {
            progress = new OnboardingProgress { TenantId = id, Status = "Started", CompletedStep = 0, UpdatedAt = DateTime.UtcNow };
            _db.OnboardingProgress.Add(progress);
        }

        progress.CompletedStep = dto.CompletedStep;
        progress.Status = dto.CompletedStep >= maxStep ? "Completed" : "InProgress";
        progress.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(BuildSetupProgress(progress, id));
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        if (_tenant.TenantId is not int id) return Forbid();
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();
        var subscription = await _db.Subscriptions.AsNoTracking().Where(s => s.TenantId == id)
            .Select(s => new { s.SubscriptionId, s.PlanId, Plan = s.Plan.Name, s.Status, s.RenewalDate }).SingleOrDefaultAsync();
        return Ok(new {
            tenant = new { tenant.TenantId, tenant.Name, tenant.Code },
            users = await _db.Users.CountAsync(u => u.TenantId == id),
            activeUsers = await _db.Users.CountAsync(u => u.TenantId == id && u.IsActive),
            roles = await _db.Roles.CountAsync(r => r.TenantId == id && !r.RolePermissions.Any(rp => rp.Permission.Name == "Platform.Tenants")),
            departments = await _db.Department.CountAsync(),
            branches = await _db.Companies.CountAsync(),
            employees = await _db.Employees.CountAsync(),
            usage = await BuildStorageUsageAsync(id),
            subscription
        });
    }

    private static TenantSetupProgressDto BuildSetupProgress(OnboardingProgress? progress, int tenantId)
    {
        var steps = TenantSetupCatalog.StepDefinitions.Select((step, index) => new TenantSetupStepDto
        {
           StepNumber = index + 1,
           Title = step.Title,
           Description = step.Description,
           IsCompleted = (progress?.CompletedStep ?? 0) >= index + 1,
           IsSkippable = step.IsSkippable
        }).ToList();

        var completedStep = Math.Clamp(progress?.CompletedStep ?? 0, 0, steps.Count);
        var totalSteps = steps.Count;
        var percentComplete = totalSteps == 0 ? 0 : (int)Math.Round((double)completedStep / totalSteps * 100);

        return new TenantSetupProgressDto
        {
           CompletedStep = completedStep,
           TotalSteps = totalSteps,
           PercentComplete = percentComplete,
           Status = completedStep >= totalSteps ? "Completed" : (completedStep > 0 ? "InProgress" : "Started"),
           UpdatedAt = progress?.UpdatedAt ?? DateTime.UtcNow,
           Steps = steps
        };
    }

    private async Task<object> BuildStorageUsageAsync(int tenantId)
    {
        var tenant = await _db.Tenants.Include(x => x.Plan).AsNoTracking().SingleAsync(x => x.TenantId == tenantId);
        var used = await _db.FileRecords.Where(x => x.TenantId == tenantId && x.IsCurrent && !x.IsDeleted && x.Status == "Active")
           .SumAsync(x => (long?)x.Size) ?? 0;
        return new { storageUsedBytes = used, storageLimitBytes = tenant.Plan.MaxStorageBytes,
           remainingBytes = Math.Max(0, tenant.Plan.MaxStorageBytes - used) };
    }

    private static class TenantSetupCatalog
    {
        public static readonly List<(string Title, string Description, bool IsSkippable)> StepDefinitions = new()
        {
           ("Company Profile", "Confirm your legal entity, trade name, country, and contact details.", false),
           ("Branches", "Add your operating branches and locations.", true),
           ("Departments", "Create each department and reporting structure.", true),
           ("Leave Policies", "Set standard leave allowances and approval requirements.", false),
           ("Working Hours", "Configure default hours, shifts, and attendance rules.", false),
           ("Payroll Settings", "Define payroll salary components and default outputs.", false),
           ("Import Employees", "Upload your employee roster and validate your data.", true),
           ("Invite Users", "Invite HR admins and managers to finish onboarding.", true)
        };
    }

    private static bool IsValidValue(TenantSettingDefinition definition, string value)
    {
        if (definition.ValueType == "boolean")
            return bool.TryParse(value, out _);
        if (definition.ValueType == "number")
            return int.TryParse(value, out var number) && number >= 0;
        if (definition.ValueType == "select")
            return definition.Options.Contains(value, StringComparer.OrdinalIgnoreCase);
        return value.Length <= 2000;
    }
}
