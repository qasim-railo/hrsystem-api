using HRSystem.API.DTOs;
using HRSystem.API.Data;
using HRSystem.API.Models;
using HRSystem.API.Services;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/numbering")]
[Authorize(Policy = "Users.Manage")]
public sealed class NumberingController : ControllerBase
{
    private readonly IReferenceNumberService _numbering;
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public NumberingController(IReferenceNumberService numbering, AppDbContext db, ICurrentTenant tenant)
    {
        _numbering = numbering;
        _db = db;
        _tenant = tenant;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NumberingPatternDto>>> List(CancellationToken cancellationToken)
    {
        var result = new List<NumberingPatternDto>();
        foreach (var definition in TenantNumberingCatalog.Definitions)
        {
            var pattern = await GetPatternAsync(definition.Key, definition.DefaultPattern, cancellationToken);
            result.Add(new NumberingPatternDto
            {
                Key = definition.Key, Label = definition.Label, Pattern = pattern,
                Preview = await _numbering.PreviewAsync(definition.Key, cancellationToken)
            });
        }
        return Ok(result);
    }

    [HttpGet("{key}/preview")]
    public async Task<ActionResult<NumberingPreviewDto>> Preview(string key, CancellationToken cancellationToken)
    {
        var definition = TenantNumberingCatalog.Definitions.SingleOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (definition is null) return NotFound();
        return Ok(new NumberingPreviewDto
        {
            Key = definition.Key, Pattern = await GetPatternAsync(key, definition.DefaultPattern, cancellationToken),
            Preview = await _numbering.PreviewAsync(key, cancellationToken)
        });
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<NumberingPatternDto>> Update(string key, NumberingPatternUpdateDto dto, CancellationToken cancellationToken)
    {
        var definition = TenantNumberingCatalog.Definitions.SingleOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (definition is null) return NotFound();
        var pattern = dto.Pattern?.Trim() ?? string.Empty;
        if (!IsValidPattern(pattern)) return BadRequest("Pattern must contain {NUMBER} and may use {YEAR} and {MONTH}.");
        var existing = await GetSettingAsync($"numbering.{definition.Key}Pattern", cancellationToken);
        if (existing is null)
        {
            if (_tenant.TenantId is not int id) return Forbid();
            existing = new TenantSetting { TenantId = id, Key = $"numbering.{definition.Key}Pattern" };
            _db.TenantSettings.Add(existing);
        }
        existing.Value = pattern;
        await _db.SaveChangesAsync(cancellationToken);
        return Ok(new NumberingPatternDto
        {
            Key = definition.Key, Label = definition.Label, Pattern = pattern,
            Preview = await _numbering.PreviewAsync(definition.Key, cancellationToken)
        });
    }

    private async Task<TenantSetting?> GetSettingAsync(string key, CancellationToken cancellationToken)
        => await _db.TenantSettings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);

    private async Task<string> GetPatternAsync(string key, string fallback, CancellationToken cancellationToken)
        => (await GetSettingAsync($"numbering.{key}Pattern", cancellationToken))?.Value ?? fallback;

    private static bool IsValidPattern(string pattern)
    {
        if (pattern.Length is 0 or > 100 || !pattern.Contains("{NUMBER}", StringComparison.OrdinalIgnoreCase))
            return false;
        var remaining = pattern.Replace("{YEAR}", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("{MONTH}", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("{NUMBER}", string.Empty, StringComparison.OrdinalIgnoreCase);
        return !remaining.Contains('{') && !remaining.Contains('}');
    }
}
