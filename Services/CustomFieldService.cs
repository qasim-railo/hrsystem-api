using System.Globalization;
using System.Text.Json;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public class CustomFieldService
{
    private readonly AppDbContext _db;
    public CustomFieldService(AppDbContext db) => _db = db;

    public async Task<List<CustomFieldDefinitionDto>> GetDefinitionsAsync(bool includeInactive = false) =>
        (await _db.CustomFieldDefinitions.AsNoTracking()
            .Where(x => x.EntityType == "Employee" && (includeInactive || x.IsActive))
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Label).ToListAsync()).Select(ToDto).ToList();

    public async Task<CustomFieldDefinitionDto?> GetDefinitionAsync(int id) =>
        (await _db.CustomFieldDefinitions.AsNoTracking().SingleOrDefaultAsync(x => x.CustomFieldDefinitionId == id)).Pipe(ToDto);

    public async Task<CustomFieldDefinitionDto> CreateDefinitionAsync(CustomFieldDefinitionDto dto)
    {
        ValidateDefinition(dto);
        if (await _db.CustomFieldDefinitions.AnyAsync(x => x.Key == dto.Key.Trim()))
            throw new InvalidOperationException("A custom field with this key already exists.");
        var entity = new CustomFieldDefinition { Key = dto.Key.Trim(), Label = dto.Label.Trim(), EntityType = "Employee", FieldType = dto.FieldType, IsRequired = dto.IsRequired, OptionsJson = JsonSerializer.Serialize(dto.Options ?? new()), DisplayOrder = dto.DisplayOrder, IsActive = dto.IsActive };
        _db.CustomFieldDefinitions.Add(entity); await _db.SaveChangesAsync(); return ToDto(entity);
    }

    public async Task<CustomFieldDefinitionDto?> UpdateDefinitionAsync(int id, CustomFieldDefinitionDto dto)
    {
        ValidateDefinition(dto);
        var entity = await _db.CustomFieldDefinitions.SingleOrDefaultAsync(x => x.CustomFieldDefinitionId == id);
        if (entity == null) return null;
        if (await _db.CustomFieldDefinitions.AnyAsync(x => x.CustomFieldDefinitionId != id && x.Key == dto.Key.Trim())) throw new InvalidOperationException("A custom field with this key already exists.");
        entity.Key = dto.Key.Trim(); entity.Label = dto.Label.Trim(); entity.FieldType = dto.FieldType; entity.IsRequired = dto.IsRequired; entity.OptionsJson = JsonSerializer.Serialize(dto.Options ?? new()); entity.DisplayOrder = dto.DisplayOrder; entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(); return ToDto(entity);
    }

    public async Task<bool> DeleteDefinitionAsync(int id)
    {
        var entity = await _db.CustomFieldDefinitions.SingleOrDefaultAsync(x => x.CustomFieldDefinitionId == id);
        if (entity == null) return false;
        entity.IsActive = false; await _db.SaveChangesAsync(); return true;
    }

    public async Task<List<CustomFieldValueDto>> GetValuesAsync(int employeeId) =>
        (await _db.CustomFieldValues.Include(x => x.Definition).Where(x => x.EmployeeId == employeeId).ToListAsync())
        .Select(x => new CustomFieldValueDto { Key = x.Definition.Key, Value = x.Value }).ToList();

    public async Task ValidateAndSaveAsync(int employeeId, IDictionary<string, string?>? values)
    {
        var definitions = await _db.CustomFieldDefinitions.Where(x => x.EntityType == "Employee" && x.IsActive).ToListAsync();
        values ??= new Dictionary<string, string?>();
        var byKey = values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions)
        {
            byKey.TryGetValue(definition.Key, out var value);
            if (definition.IsRequired && string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"Custom field '{definition.Label}' is required.");
            if (!string.IsNullOrWhiteSpace(value)) ValidateValue(definition, value!);
        }
        var existing = await _db.CustomFieldValues.Where(x => x.EmployeeId == employeeId).ToListAsync();
        foreach (var pair in byKey)
        {
            var definition = definitions.FirstOrDefault(x => x.Key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
            if (definition == null) throw new ArgumentException($"Unknown or inactive custom field '{pair.Key}'.");
            var current = existing.FirstOrDefault(x => x.CustomFieldDefinitionId == definition.CustomFieldDefinitionId);
            if (string.IsNullOrWhiteSpace(pair.Value)) { if (current != null) _db.CustomFieldValues.Remove(current); }
            else if (current == null) _db.CustomFieldValues.Add(new CustomFieldValue { EmployeeId = employeeId, CustomFieldDefinitionId = definition.CustomFieldDefinitionId, Value = pair.Value! });
            else current.Value = pair.Value!;
        }
        await _db.SaveChangesAsync();
    }

    private static void ValidateDefinition(CustomFieldDefinitionDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Key) || !System.Text.RegularExpressions.Regex.IsMatch(dto.Key, "^[A-Za-z][A-Za-z0-9_]*$")) throw new ArgumentException("Key must start with a letter and contain only letters, numbers and underscores.");
        if (string.IsNullOrWhiteSpace(dto.Label)) throw new ArgumentException("Label is required.");
        if ((dto.FieldType == CustomFieldType.Dropdown || dto.FieldType == CustomFieldType.MultiSelect) && (dto.Options == null || dto.Options.Count == 0)) throw new ArgumentException("Options are required for dropdown fields.");
    }
    private static void ValidateValue(CustomFieldDefinition d, string value)
    {
        var options = JsonSerializer.Deserialize<List<string>>(d.OptionsJson) ?? new();
        switch (d.FieldType)
        {
            case CustomFieldType.Number when !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _): throw new ArgumentException($"'{d.Label}' must be a number.");
            case CustomFieldType.Currency when !decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _): throw new ArgumentException($"'{d.Label}' must be a currency amount.");
            case CustomFieldType.Date when !DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _): throw new ArgumentException($"'{d.Label}' must be a valid date.");
            case CustomFieldType.Boolean or CustomFieldType.Checkbox when !bool.TryParse(value, out _): throw new ArgumentException($"'{d.Label}' must be true or false.");
            case CustomFieldType.Dropdown when !options.Contains(value, StringComparer.OrdinalIgnoreCase): throw new ArgumentException($"'{d.Label}' has an invalid option.");
            case CustomFieldType.MultiSelect:
                if (JsonSerializer.Deserialize<List<string>>(value) is not { } selected || selected.Any(x => !options.Contains(x, StringComparer.OrdinalIgnoreCase))) throw new ArgumentException($"'{d.Label}' has invalid options.");
                break;
        }
    }
    private static CustomFieldDefinitionDto ToDto(CustomFieldDefinition x) => new() { CustomFieldDefinitionId = x.CustomFieldDefinitionId, Key = x.Key, Label = x.Label, EntityType = x.EntityType, FieldType = x.FieldType, IsRequired = x.IsRequired, Options = JsonSerializer.Deserialize<List<string>>(x.OptionsJson) ?? new(), DisplayOrder = x.DisplayOrder, IsActive = x.IsActive };
}
internal static class ObjectExtensions { public static TResult? Pipe<T, TResult>(this T? value, Func<T, TResult> f) where T : class => value == null ? default : f(value); }
