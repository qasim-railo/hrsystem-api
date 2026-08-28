using System.Globalization;
using System.Text.RegularExpressions;
using HRSystem.API.Data;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class ReferenceNumberService : IReferenceNumberService
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;

    public ReferenceNumberService(AppDbContext db, ICurrentTenant tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<string> NextAsync(string sequenceKey, CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenant();
        var definition = await ResolveDefinitionAsync(sequenceKey, cancellationToken);
        var year = DateTime.UtcNow.Year;
        await using var transaction = await _db.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable, cancellationToken);
        var sequence = await _db.NumberingSequences.SingleOrDefaultAsync(
            x => x.TenantId == tenantId && x.SequenceKey == definition.Key && x.Year == year,
            cancellationToken);
        if (sequence is null)
        {
            sequence = new NumberingSequence { TenantId = tenantId, SequenceKey = definition.Key, Year = year, LastNumber = 1 };
            _db.NumberingSequences.Add(sequence);
        }
        else
        {
            sequence.LastNumber++;
        }
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Format(definition.DefaultPattern, year, sequence.LastNumber);
    }

    public async Task<string> PreviewAsync(string sequenceKey, CancellationToken cancellationToken = default)
    {
        RequireTenant();
        var definition = await ResolveDefinitionAsync(sequenceKey, cancellationToken);
        var year = DateTime.UtcNow.Year;
        var current = await _db.NumberingSequences.AsNoTracking().SingleOrDefaultAsync(
            x => x.TenantId == _tenant.TenantId && x.SequenceKey == definition.Key && x.Year == year,
            cancellationToken);
        return Format(definition.DefaultPattern, year, (current?.LastNumber ?? 0) + 1);
    }

    private int RequireTenant()
        => _tenant.TenantId is int tenantId && tenantId > 0
            ? tenantId
            : throw new UnauthorizedAccessException("A tenant context is required.");

    private async Task<NumberingDefinition> ResolveDefinitionAsync(string key, CancellationToken cancellationToken)
    {
        var definition = TenantNumberingCatalog.Definitions.SingleOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unknown numbering key '{key}'.");
        var overrideSetting = await _db.TenantSettings.AsNoTracking().SingleOrDefaultAsync(
            x => x.Key == $"numbering.{definition.Key}Pattern", cancellationToken);
        return overrideSetting is null || string.IsNullOrWhiteSpace(overrideSetting.Value)
            ? definition
            : definition with { DefaultPattern = overrideSetting.Value };
    }

    public static string Format(string pattern, int year, int number)
        => pattern.Replace("{YEAR}", year.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{MONTH}", DateTime.UtcNow.Month.ToString("00", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{NUMBER}", number.ToString("0000", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase);
}
