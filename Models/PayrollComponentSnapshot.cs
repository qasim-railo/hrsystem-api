namespace HRSystem.API.Models;

public class PayrollComponentSnapshot : ITenantOwned
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public int PayrollId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ConfiguredValue { get; set; }
    public string CalculationType { get; set; } = string.Empty;
    public bool IsTaxable { get; set; }
    public bool IsPensionable { get; set; }
    public bool IsWpsIncluded { get; set; }
    public Payroll Payroll { get; set; } = null!;
}
