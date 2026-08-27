namespace HRSystem.API.Models;

public class CustomFieldValue : ITenantOwned
{
    public int CustomFieldValueId { get; set; }
    public int TenantId { get; set; }
    public int EmployeeId { get; set; }
    public int CustomFieldDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
    public Employee Employee { get; set; } = null!;
    public CustomFieldDefinition Definition { get; set; } = null!;
}
