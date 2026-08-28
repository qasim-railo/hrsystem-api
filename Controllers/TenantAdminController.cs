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

    private async Task<object> BuildStorageUsageAsync(int tenantId)
    {
        var tenant = await _db.Tenants.Include(x => x.Plan).AsNoTracking().SingleAsync(x => x.TenantId == tenantId);
        var used = await _db.FileRecords.Where(x => x.TenantId == tenantId && x.IsCurrent && !x.IsDeleted && x.Status == "Active")
           .SumAsync(x => (long?)x.Size) ?? 0;
        return new { storageUsedBytes = used, storageLimitBytes = tenant.Plan.MaxStorageBytes,
           remainingBytes = Math.Max(0, tenant.Plan.MaxStorageBytes - used) };
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
