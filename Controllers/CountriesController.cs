using HRSystem.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/countries")]
public class CountriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CountriesController(AppDbContext db) => _db = db;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List()
    {
        var countries = await _db.Countries.AsNoTracking()
            .Where(country => country.IsActive)
            .OrderBy(country => country.Name)
            .Select(country => new { country.CountryId, country.Code, country.Name })
            .ToListAsync();

        return Ok(countries);
    }

    [HttpGet("currencies")]
    [AllowAnonymous]
    public async Task<IActionResult> ListCurrencies()
    {
        return Ok(await _db.Currencies.AsNoTracking().Where(currency => currency.IsActive)
            .OrderBy(currency => currency.Code)
            .Select(currency => new { currency.CurrencyId, currency.Code, currency.Name, currency.Symbol, currency.DecimalPlaces })
            .ToListAsync());
    }

    [HttpGet("time-zones")]
    [AllowAnonymous]
    public async Task<IActionResult> ListTimeZones([FromQuery] string? countryCode)
    {
        var timeZones = _db.TimeZones.AsNoTracking().Where(timeZone => timeZone.IsActive);
        if (!string.IsNullOrWhiteSpace(countryCode))
            timeZones = timeZones.Where(timeZone => timeZone.CountryCode == countryCode.ToUpper());
        return Ok(await timeZones.OrderBy(timeZone => timeZone.DisplayName)
            .Select(timeZone => new { timeZone.TimeZoneId, timeZone.DisplayName, timeZone.CountryCode })
            .ToListAsync());
    }
}
