namespace HRSystem.API.DTOs;

public sealed class NumberingPatternDto
{
    public string Key { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Pattern { get; init; } = string.Empty;
    public string Preview { get; init; } = string.Empty;
}

public sealed class NumberingPatternUpdateDto
{
    public string? Pattern { get; set; }
}

public sealed class NumberingPreviewDto
{
    public string Key { get; init; } = string.Empty;
    public string Pattern { get; init; } = string.Empty;
    public string Preview { get; init; } = string.Empty;
}
