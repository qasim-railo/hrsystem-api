using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Authorize(Policy = "Users.Manage")]
[Route("api/payroll-components")]
public class PayrollComponentsController : ControllerBase
{
    private readonly AppDbContext _db;
    public PayrollComponentsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollComponentDto>>> List()
    {
        if (!await _db.PayrollComponents.AnyAsync())
        {
            foreach (var item in Defaults)
                _db.PayrollComponents.Add(new PayrollComponent { Code = item.Code, Name = item.Name, ComponentType = item.Type, SalaryField = item.Field });
            await _db.SaveChangesAsync();
        }
        return Ok(await _db.PayrollComponents.AsNoTracking().OrderBy(x => x.ComponentType).ThenBy(x => x.Name).Select(MapExpression).ToListAsync());
    }

    [HttpPost]
    public async Task<ActionResult<PayrollComponentDto>> Create(SavePayrollComponentDto dto)
    {
        var validation = Validate(dto);
        if (validation != null) return BadRequest(validation);
        var component = new PayrollComponent();
        Apply(component, dto);
        _db.PayrollComponents.Add(component);
        await _db.SaveChangesAsync();
        return Ok(Map(component));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PayrollComponentDto>> Update(int id, SavePayrollComponentDto dto)
    {
        var validation = Validate(dto);
        if (validation != null) return BadRequest(validation);
        var component = await _db.PayrollComponents.SingleOrDefaultAsync(x => x.Id == id);
        if (component == null) return NotFound();
        Apply(component, dto);
        await _db.SaveChangesAsync();
        return Ok(Map(component));
    }

    private static string? Validate(SavePayrollComponentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name)) return "Code and name are required.";
        if (dto.ComponentType is not ("Earning" or "Deduction")) return "Component type must be Earning or Deduction.";
        if (dto.CalculationType is not ("Fixed" or "Percentage")) return "Calculation type must be Fixed or Percentage.";
        if (dto.Value < 0 || (dto.CalculationType == "Percentage" && dto.Value > 100)) return "Value must be valid.";
        return null;
    }
    private static void Apply(PayrollComponent x, SavePayrollComponentDto d)
    {
        x.Code = d.Code.Trim().ToUpperInvariant(); x.Name = d.Name.Trim(); x.ComponentType = d.ComponentType;
        x.CalculationType = d.CalculationType; x.Value = d.Value; x.SalaryField = d.SalaryField?.Trim() ?? string.Empty;
        x.BaseComponentCode = d.BaseComponentCode?.Trim().ToUpperInvariant() ?? string.Empty; x.IsTaxable = d.IsTaxable;
        x.IsPensionable = d.IsPensionable; x.IsWpsIncluded = d.IsWpsIncluded; x.IsActive = d.IsActive;
    }
    private static PayrollComponentDto Map(PayrollComponent x) => new() { Id = x.Id, Code = x.Code, Name = x.Name, ComponentType = x.ComponentType, CalculationType = x.CalculationType, Value = x.Value, SalaryField = x.SalaryField, BaseComponentCode = x.BaseComponentCode, IsTaxable = x.IsTaxable, IsPensionable = x.IsPensionable, IsWpsIncluded = x.IsWpsIncluded, IsActive = x.IsActive };
    private static readonly System.Linq.Expressions.Expression<Func<PayrollComponent, PayrollComponentDto>> MapExpression = x => new PayrollComponentDto { Id = x.Id, Code = x.Code, Name = x.Name, ComponentType = x.ComponentType, CalculationType = x.CalculationType, Value = x.Value, SalaryField = x.SalaryField, BaseComponentCode = x.BaseComponentCode, IsTaxable = x.IsTaxable, IsPensionable = x.IsPensionable, IsWpsIncluded = x.IsWpsIncluded, IsActive = x.IsActive };
    private static readonly (string Code, string Name, string Type, string Field)[] Defaults =
    {
        ("BASIC_SALARY", "Basic Salary", "Earning", "BasicSalary"),
        ("ACCOMMODATION", "Accommodation", "Earning", "Accommodation"),
        ("TRANSPORTATION", "Transportation", "Earning", "Transportation"),
        ("FOOD", "Food", "Earning", "Food"),
        ("OTHER_ALLOWANCE", "Other Allowance", "Earning", "OtherAllowance"),
        ("OVERTIME", "Overtime", "Earning", "Overtime"),
        ("BONUS", "Bonus", "Earning", "Bonus"),
        ("COMMISSION", "Commission", "Earning", "Commission"),
        ("LOAN_DEDUCTION", "Loan Deduction", "Deduction", "LoanDeduction"),
        ("ABSENCE_DEDUCTION", "Absence Deduction", "Deduction", "AbsenceDeduction"),
        ("OTHER_DEDUCTION", "Other Deduction", "Deduction", "OtherDeduction")
    };
}
