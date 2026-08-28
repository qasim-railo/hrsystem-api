using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRSystem.API.Controllers;

[ApiController, Authorize]
[Route("api/language")]
public class LanguageController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public LanguageController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!int.TryParse(User.FindFirstValue("user_id"), out var userId) || _tenant.TenantId is not int tenantId)
            return Forbid();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId);
        var tenant = await _db.Tenants.AsNoTracking().SingleOrDefaultAsync(x => x.TenantId == tenantId);
        if (user == null || tenant == null) return Forbid();
        var userLanguage = IsSupported(user.Language) ? user.Language.ToLowerInvariant() : tenant.DefaultLanguage;
        var tenantLanguage = IsSupported(tenant.DefaultLanguage) ? tenant.DefaultLanguage.ToLowerInvariant() : "en";
        var changed = user.Language != userLanguage || tenant.DefaultLanguage != tenantLanguage;
        user.Language = userLanguage;
        tenant.DefaultLanguage = tenantLanguage;
        if (changed) await _db.SaveChangesAsync();
        return Ok(new { userLanguage, tenantDefaultLanguage = tenantLanguage });
    }

    [HttpPut("user")]
    public async Task<IActionResult> SetUser(LanguagePreferenceDto dto)
    {
        if (!IsSupported(dto.Language) || !int.TryParse(User.FindFirstValue("user_id"), out var userId))
            return BadRequest("Language must be 'en' or 'ar'.");
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user == null) return Forbid();
        user.Language = dto.Language.ToLowerInvariant();
        await _db.SaveChangesAsync();
        return Ok(new { language = user.Language });
    }

    [HttpPut("tenant")]
    [Authorize(Policy = "Users.Manage")]
    public async Task<IActionResult> SetTenant(LanguagePreferenceDto dto)
    {
        if (!IsSupported(dto.Language) || _tenant.TenantId is not int tenantId)
            return BadRequest("Language must be 'en' or 'ar'.");
        var tenant = await _db.Tenants.SingleOrDefaultAsync(x => x.TenantId == tenantId);
        if (tenant == null) return NotFound();
        tenant.DefaultLanguage = dto.Language.ToLowerInvariant();
        await _db.SaveChangesAsync();
        return Ok(new { language = tenant.DefaultLanguage });
    }

    private static bool IsSupported(string? language) => language is "en" or "ar" or "EN" or "AR";
}
