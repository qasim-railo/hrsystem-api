namespace HRSystem.API.Models;

public enum CustomFieldType
{
    Text, Number, Date, Dropdown, MultiSelect, Checkbox, Boolean, Currency, File
}

public class CustomFieldDefinition : ITenantOwned
{
    public int CustomFieldDefinitionId { get; set; }
    public int TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string EntityType { get; set; } = "Employee";
    public CustomFieldType FieldType { get; set; }
    public bool IsRequired { get; set; }
    public string OptionsJson { get; set; } = "[]";
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<CustomFieldValue> Values { get; set; } = new List<CustomFieldValue>();
}
