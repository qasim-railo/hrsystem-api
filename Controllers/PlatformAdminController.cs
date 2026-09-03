using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/platform")]
[Authorize(Policy = "Platform.Tenants")]
public class PlatformAdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlatformAdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<IEnumerable<PlatformTenantDto>>> GetTenants([FromQuery] string? search)
    {
        var tenants = _db.Tenants.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            tenants = tenants.Where(t => t.Name.Contains(search) || t.Code.Contains(search));

        var result = await tenants
            .Select(t => new PlatformTenantDto
            {
                TenantId = t.TenantId,
                Name = t.Name,
                Code = t.Code,
                Status = t.Status,
                LifecycleStatus = t.LifecycleStatus,
                PlanName = t.PlanName,
                TrialStartDate = t.TrialStartDate,
                TrialEndDate = t.TrialEndDate,
                TrialDaysRemaining = PlatformTenantDto.CalculateTrialDaysRemaining(t.TrialEndDate, t.Status, t.LifecycleStatus),
                BillingStatus = t.BillingStatus,
                StorageUsedBytes = _db.FileRecords.Where(f => f.TenantId == t.TenantId && f.IsCurrent && !f.IsDeleted && f.Status == "Active").Sum(f => (long?)f.Size) ?? 0,
                StorageLimitBytes = t.Plan.MaxStorageBytes,
                UserCount = _db.Users.Count(u => u.TenantId == t.TenantId),
                EmployeeCount = _db.Employees.IgnoreQueryFilters().Count(e => e.TenantId == t.TenantId),
                CompanyCount = _db.Companies.IgnoreQueryFilters().Count(c => c.TenantId == t.TenantId)
            })
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("tenants/{id:int}")]
    public async Task<ActionResult<PlatformTenantDto>> GetTenant(int id)
    {
        var tenant = await BuildTenantQuery().SingleOrDefaultAsync(t => t.TenantId == id);
        return tenant == null ? NotFound() : Ok(tenant);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<PlatformStatisticsDto>> GetStatistics()
    {
        return Ok(new PlatformStatisticsDto
        {
            TotalTenants = await _db.Tenants.CountAsync(),
            ActiveTenants = await _db.Tenants.CountAsync(t => t.Status == "Active"),
            TrialTenants = await _db.Tenants.CountAsync(t => t.Status == "Trial"),
            SuspendedTenants = await _db.Tenants.CountAsync(t => t.Status == "Suspended"),
            TotalUsers = await _db.Users.CountAsync(),
            TotalEmployees = await _db.Employees.IgnoreQueryFilters().CountAsync(),
            TotalStorageUsedBytes = await _db.FileRecords.Where(f => f.IsCurrent && !f.IsDeleted && f.Status == "Active").SumAsync(f => (long?)f.Size) ?? 0
        });
    }

    [HttpGet("master-data/countries")]
    public async Task<ActionResult<IEnumerable<CountryMasterDto>>> GetCountries() =>
        Ok(await _db.Countries.AsNoTracking().OrderBy(country => country.Name).Select(country => new CountryMasterDto
        {
            CountryId = country.CountryId, Code = country.Code, Name = country.Name, IsActive = country.IsActive
        }).ToListAsync());

    [HttpPost("master-data/countries")]
    public async Task<ActionResult<CountryMasterDto>> CreateCountry(CountryMasterDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _db.Countries.AnyAsync(country => country.Code == code))
            return Conflict("A country with this ISO code already exists.");
        var country = new Country { Code = code, Name = dto.Name.Trim(), IsActive = dto.IsActive };
        _db.Countries.Add(country);
        await LogMasterDataChangeAsync("Created", nameof(Country), code);
        await _db.SaveChangesAsync();
        dto.CountryId = country.CountryId;
        dto.Code = country.Code;
        dto.Name = country.Name;
        return CreatedAtAction(nameof(GetCountries), dto);
    }

    [HttpPut("master-data/countries/{id:int}")]
    public async Task<ActionResult<CountryMasterDto>> UpdateCountry(int id, CountryMasterDto dto)
    {
        var country = await _db.Countries.SingleOrDefaultAsync(item => item.CountryId == id);
        if (country == null) return NotFound();
        var code = dto.Code.Trim().ToUpperInvariant();
        if (!string.Equals(country.Code, code, StringComparison.Ordinal) && await _db.Tenants.AnyAsync(tenant => tenant.CountryCode == country.Code))
            return Conflict("A country selected by a tenant cannot have its ISO code changed.");
        country.Code = code;
        country.Name = dto.Name.Trim();
        country.IsActive = dto.IsActive;
        await LogMasterDataChangeAsync("Updated", nameof(Country), country.Code);
        await _db.SaveChangesAsync();
        dto.CountryId = country.CountryId;
        dto.Code = country.Code;
        dto.Name = country.Name;
        return Ok(dto);
    }

    [HttpDelete("master-data/countries/{id:int}")]
    public async Task<IActionResult> DeleteCountry(int id)
    {
        var country = await _db.Countries.SingleOrDefaultAsync(item => item.CountryId == id);
        if (country == null) return NotFound();
        if (await _db.Tenants.AnyAsync(tenant => tenant.DefaultCountryId == id))
            return Conflict("A country selected by a tenant cannot be deleted. Deactivate it instead.");
        _db.Countries.Remove(country);
        await LogMasterDataChangeAsync("Deleted", nameof(Country), country.Code);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("master-data/currencies")]
    public async Task<ActionResult<IEnumerable<CurrencyMasterDto>>> GetCurrencies() =>
        Ok(await _db.Currencies.AsNoTracking().OrderBy(currency => currency.Code).Select(currency => new CurrencyMasterDto
        {
            CurrencyId = currency.CurrencyId, Code = currency.Code, Name = currency.Name, Symbol = currency.Symbol, DecimalPlaces = currency.DecimalPlaces, IsActive = currency.IsActive
        }).ToListAsync());

    [HttpPost("master-data/currencies")]
    public async Task<ActionResult<CurrencyMasterDto>> CreateCurrency(CurrencyMasterDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _db.Currencies.AnyAsync(currency => currency.Code == code))
            return Conflict("A currency with this code already exists.");
        var currency = new Currency { Code = code, Name = dto.Name.Trim(), Symbol = dto.Symbol.Trim(), DecimalPlaces = dto.DecimalPlaces, IsActive = dto.IsActive };
        _db.Currencies.Add(currency);
        await LogMasterDataChangeAsync("Created", nameof(Currency), code);
        await _db.SaveChangesAsync();
        dto.CurrencyId = currency.CurrencyId;
        dto.Code = currency.Code;
        return CreatedAtAction(nameof(GetCurrencies), dto);
    }

    [HttpPut("master-data/currencies/{id:int}")]
    public async Task<ActionResult<CurrencyMasterDto>> UpdateCurrency(int id, CurrencyMasterDto dto)
    {
        var currency = await _db.Currencies.SingleOrDefaultAsync(item => item.CurrencyId == id);
        if (currency == null) return NotFound();
        var code = dto.Code.Trim().ToUpperInvariant();
        if (!string.Equals(currency.Code, code, StringComparison.Ordinal) &&
            (await _db.Tenants.AnyAsync(tenant => tenant.DefaultCurrencyId == id) || await _db.TenantCurrencies.AnyAsync(item => item.CurrencyId == id)))
            return Conflict("A currency enabled by a tenant cannot have its code changed.");
        currency.Code = code;
        currency.Name = dto.Name.Trim();
        currency.Symbol = dto.Symbol.Trim();
        currency.DecimalPlaces = dto.DecimalPlaces;
        currency.IsActive = dto.IsActive;
        await LogMasterDataChangeAsync("Updated", nameof(Currency), currency.Code);
        await _db.SaveChangesAsync();
        dto.CurrencyId = currency.CurrencyId;
        dto.Code = currency.Code;
        return Ok(dto);
    }

    [HttpDelete("master-data/currencies/{id:int}")]
    public async Task<IActionResult> DeleteCurrency(int id)
    {
        var currency = await _db.Currencies.SingleOrDefaultAsync(item => item.CurrencyId == id);
        if (currency == null) return NotFound();
        if (await _db.Tenants.AnyAsync(tenant => tenant.DefaultCurrencyId == id) || await _db.TenantCurrencies.AnyAsync(item => item.CurrencyId == id))
            return Conflict("A currency enabled by a tenant cannot be deleted. Deactivate it instead.");
        _db.Currencies.Remove(currency);
        await LogMasterDataChangeAsync("Deleted", nameof(Currency), currency.Code);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("master-data/time-zones")]
    public async Task<ActionResult<IEnumerable<TimeZoneMasterDto>>> GetTimeZones() =>
        Ok(await _db.TimeZones.AsNoTracking().OrderBy(timeZone => timeZone.DisplayName).Select(timeZone => new TimeZoneMasterDto
        {
            TimeZoneId = timeZone.TimeZoneId, DisplayName = timeZone.DisplayName, CountryCode = timeZone.CountryCode, IsActive = timeZone.IsActive
        }).ToListAsync());

    [HttpPost("master-data/time-zones")]
    public async Task<ActionResult<TimeZoneMasterDto>> CreateTimeZone(TimeZoneMasterDto dto)
    {
        var id = dto.TimeZoneId.Trim();
        if (await _db.TimeZones.AnyAsync(timeZone => timeZone.TimeZoneId == id))
            return Conflict("A time zone with this identifier already exists.");
        if (string.IsNullOrWhiteSpace(dto.CountryCode) ||
            !await _db.Countries.AnyAsync(country => country.Code == dto.CountryCode.Trim().ToUpperInvariant()))
            return BadRequest("Select a PeopleOS country for the time zone.");
        var timeZone = new TimeZoneMaster { TimeZoneId = id, DisplayName = dto.DisplayName.Trim(), CountryCode = dto.CountryCode?.Trim().ToUpperInvariant(), IsActive = dto.IsActive };
        _db.TimeZones.Add(timeZone);
        await LogMasterDataChangeAsync("Created", nameof(TimeZoneMaster), id);
        await _db.SaveChangesAsync();
        dto.TimeZoneId = timeZone.TimeZoneId;
        return CreatedAtAction(nameof(GetTimeZones), dto);
    }

    [HttpPut("master-data/time-zones/{**timeZoneId}")]
    public async Task<ActionResult<TimeZoneMasterDto>> UpdateTimeZone(string timeZoneId, TimeZoneMasterDto dto)
    {
        if (!string.Equals(timeZoneId, dto.TimeZoneId, StringComparison.Ordinal))
            return BadRequest("Time zone identifiers cannot be changed.");
        var timeZone = await _db.TimeZones.SingleOrDefaultAsync(item => item.TimeZoneId == timeZoneId);
        if (timeZone == null) return NotFound();
        if (string.IsNullOrWhiteSpace(dto.CountryCode) ||
            !await _db.Countries.AnyAsync(country => country.Code == dto.CountryCode.Trim().ToUpperInvariant()))
            return BadRequest("Select a PeopleOS country for the time zone.");
        timeZone.DisplayName = dto.DisplayName.Trim();
        timeZone.CountryCode = dto.CountryCode?.Trim().ToUpperInvariant();
        timeZone.IsActive = dto.IsActive;
        await LogMasterDataChangeAsync("Updated", nameof(TimeZoneMaster), timeZone.TimeZoneId);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpDelete("master-data/time-zones/{**timeZoneId}")]
    public async Task<IActionResult> DeleteTimeZone(string timeZoneId)
    {
        var timeZone = await _db.TimeZones.SingleOrDefaultAsync(item => item.TimeZoneId == timeZoneId);
        if (timeZone == null) return NotFound();
        if (await _db.Tenants.AnyAsync(tenant => tenant.DefaultTimeZoneId == timeZoneId))
            return Conflict("A tenant's default time zone cannot be deleted. Deactivate it instead.");
        _db.TimeZones.Remove(timeZone);
        await LogMasterDataChangeAsync("Deleted", nameof(TimeZoneMaster), timeZoneId);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<IEnumerable<PlatformAuditLogDto>>> GetAuditLogs([FromQuery] int limit = 100)
    {
        var safeLimit = Math.Clamp(limit, 1, 500);
        var logs = await _db.PlatformAuditLogs.AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(safeLimit)
            .GroupJoin(
                _db.Tenants.AsNoTracking(),
                log => log.TenantId,
                tenant => tenant.TenantId,
                (log, tenants) => new { log, tenants })
            .SelectMany(
                item => item.tenants.DefaultIfEmpty(),
                (item, tenant) => new PlatformAuditLogDto
                {
                    Id = item.log.Id,
                    TenantId = item.log.TenantId,
                    TenantName = tenant == null ? "PeopleOS platform" : tenant.Name,
                    Action = item.log.Action,
                    Entity = item.log.Entity,
                    EntityId = item.log.EntityId,
                    UserId = item.log.UserId,
                    Details = item.log.Details,
                    CreatedAt = item.log.CreatedAt
                })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpPost("tenants/{id:int}/activate")]
    public Task<IActionResult> Activate(int id) => ChangeStatus(id, "Active", "Activated");

    [HttpPost("tenants/{id:int}/suspend")]
    public Task<IActionResult> Suspend(int id) => ChangeStatus(id, "Suspended", "Suspended");

    [HttpPost("tenants/{id:int}/resume")]
    public Task<IActionResult> Resume(int id) => ChangeStatus(id, "Active", "Resumed");

    [HttpPost("tenants/{id:int}/archive")]
    public Task<IActionResult> Archive(int id) => ChangeStatus(id, "Archived", "Archived");

    private async Task<IActionResult> ChangeStatus(int id, string status, string action)
    {
        var tenant = await _db.Tenants.SingleOrDefaultAsync(t => t.TenantId == id);
        if (tenant == null) return NotFound();

        tenant.Status = status;
        tenant.LifecycleStatus = status;
        tenant.BillingStatus = status;
        await UpdateSubscriptionFromTenantLifecycleAsync(tenant, status);

        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            TenantId = id,
            Action = action,
            Entity = nameof(Tenant),
            EntityId = id.ToString(),
            UserId = User.Identity?.Name ?? "unknown",
            Details = $"Tenant status changed to {status}."
        });
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task LogMasterDataChangeAsync(string action, string entity, string entityId)
    {
        var platformTenantId = await _db.Tenants
            .Where(tenant => tenant.Code == "PEOPLEOS")
            .Select(tenant => tenant.TenantId)
            .SingleAsync();

        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            TenantId = platformTenantId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            UserId = User.Identity?.Name ?? "unknown",
            Details = $"{entity} master data {action.ToLowerInvariant()}."
        });
        await Task.CompletedTask;
    }

    private async Task UpdateSubscriptionFromTenantLifecycleAsync(Tenant tenant, string status)
    {
        var subscription = await _db.Subscriptions.SingleOrDefaultAsync(s => s.TenantId == tenant.TenantId);
        if (subscription == null)
            return;

        switch (status)
        {
            case "Trial":
                subscription.Status = SubscriptionStatus.Trial;
                subscription.TrialStartDate ??= tenant.TrialStartDate ?? DateTime.UtcNow;
                subscription.TrialEndDate ??= tenant.TrialEndDate ?? DateTime.UtcNow.AddDays(14);
                subscription.StartDate = subscription.StartDate == default ? (tenant.TrialStartDate ?? DateTime.UtcNow) : subscription.StartDate;
                break;
            case "Active":
                subscription.Status = SubscriptionStatus.Active;
                subscription.TrialStartDate ??= tenant.TrialStartDate ?? DateTime.UtcNow;
                subscription.TrialEndDate ??= tenant.TrialEndDate ?? DateTime.UtcNow.AddDays(14);
                subscription.StartDate = subscription.StartDate == default ? (tenant.TrialStartDate ?? DateTime.UtcNow) : subscription.StartDate;
                subscription.RenewalDate ??= subscription.TrialEndDate ?? DateTime.UtcNow.AddMonths(1);
                break;
            case "Suspended":
                subscription.Status = SubscriptionStatus.Suspended;
                break;
            case "Archived":
                subscription.Status = SubscriptionStatus.Cancelled;
                subscription.CancelledAt ??= DateTime.UtcNow;
                break;
            case "Resume":
            case "Resumed":
                subscription.Status = SubscriptionStatus.Active;
                break;
        }

        subscription.UpdatedAt = DateTime.UtcNow;
    }

    private IQueryable<PlatformTenantDto> BuildTenantQuery()
    {
        return _db.Tenants.AsNoTracking().Select(t => new PlatformTenantDto
        {
            TenantId = t.TenantId,
            Name = t.Name,
            Code = t.Code,
            Status = t.Status,
            LifecycleStatus = t.LifecycleStatus,
            PlanName = t.PlanName,
            TrialStartDate = t.TrialStartDate,
            TrialEndDate = t.TrialEndDate,
            TrialDaysRemaining = PlatformTenantDto.CalculateTrialDaysRemaining(t.TrialEndDate, t.Status, t.LifecycleStatus),
            BillingStatus = t.BillingStatus,
            StorageUsedBytes = _db.FileRecords.Where(f => f.TenantId == t.TenantId && f.IsCurrent && !f.IsDeleted && f.Status == "Active").Sum(f => (long?)f.Size) ?? 0,
            StorageLimitBytes = t.Plan.MaxStorageBytes,
            UserCount = _db.Users.Count(u => u.TenantId == t.TenantId),
            EmployeeCount = _db.Employees.IgnoreQueryFilters().Count(e => e.TenantId == t.TenantId),
            CompanyCount = _db.Companies.IgnoreQueryFilters().Count(c => c.TenantId == t.TenantId)
        });
    }
}
