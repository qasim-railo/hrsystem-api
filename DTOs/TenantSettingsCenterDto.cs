namespace HRSystem.API.DTOs;

public sealed class TenantSettingsCenterDto
{
    public IReadOnlyList<TenantSettingsSectionDto> Sections { get; init; } = [];
}

public sealed class TenantSettingsSectionDto
{
    public string Key { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<TenantSettingItemDto> Settings { get; init; } = [];
}

public sealed class TenantSettingItemDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string ValueType { get; init; } = "text";
    public string Value { get; init; } = string.Empty;
    public string DefaultValue { get; init; } = string.Empty;
    public bool IsOverridden { get; init; }
    public IReadOnlyList<string> Options { get; init; } = [];
}
