using HRSystem.API.Models;

namespace HRSystem.API.DTOs;

public class CustomFieldDefinitionDto
{
    public int CustomFieldDefinitionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EntityType { get; set; } = "Employee";
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CustomFieldValueDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}
