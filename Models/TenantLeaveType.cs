namespace HRSystem.API.Models;

public class TenantLeaveType : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DefaultDays { get; set; }
    public bool IsActive { get; set; } = true;
}
