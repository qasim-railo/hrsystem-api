namespace HRSystem.API.DTOs;

public class PayrollComponentDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ComponentType { get; set; } = "Earning";
    public string CalculationType { get; set; } = "Fixed";
    public decimal Value { get; set; }
    public string SalaryField { get; set; } = string.Empty;
    public string BaseComponentCode { get; set; } = string.Empty;
    public bool IsTaxable { get; set; }
    public bool IsPensionable { get; set; }
    public bool IsWpsIncluded { get; set; }
    public bool IsActive { get; set; }
}

public class SavePayrollComponentDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ComponentType { get; set; } = "Earning";
    public string CalculationType { get; set; } = "Fixed";
    public decimal Value { get; set; }
    public string SalaryField { get; set; } = string.Empty;
    public string BaseComponentCode { get; set; } = string.Empty;
    public bool IsTaxable { get; set; }
    public bool IsPensionable { get; set; }
    public bool IsWpsIncluded { get; set; }
    public bool IsActive { get; set; } = true;
}
