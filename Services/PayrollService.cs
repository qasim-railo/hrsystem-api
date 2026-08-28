using AutoMapper;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HRSystem.API.Services
{
    // PayrollService.cs
    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public PayrollService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<PayrollDto>> GenerateMonthlyPayrollAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var employees = await _context.Employees.Include(x => x.EmploymentDetail).ToListAsync();
            var components = await EnsureDefaultComponentsAsync();
            var payrolls = new List<Payroll>();

            foreach (var emp in employees)
            {
                var attendance = await _context.Attendance
                    .Where(a => a.EmployeeId == emp.EmployeeId && a.Date >= startDate && a.Date <= endDate)
                    .ToListAsync();

                var ot1 = attendance.Sum(a => a.OT1);
                var ot2 = attendance.Sum(a => a.OT2);
                var otEarnings = (ot1 * 20) + (ot2 * 30);
                var salary = emp.EmploymentDetail?.BasicSalary ?? 2000m;
                var values = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    ["BASIC_SALARY"] = salary,
                    ["ACCOMMODATION"] = emp.EmploymentDetail?.AccommodationAllowance ?? 0m,
                    ["TRANSPORTATION"] = emp.EmploymentDetail?.TravelAllowance ?? 0m,
                    ["FOOD"] = 0m,
                    ["OTHER_ALLOWANCE"] = emp.EmploymentDetail?.OtherAllowance ?? 0m,
                    ["OVERTIME"] = (decimal)otEarnings,
                    ["BONUS"] = 0m,
                    ["COMMISSION"] = 0m,
                    ["LOAN_DEDUCTION"] = 0m,
                    ["ABSENCE_DEDUCTION"] = 0m,
                    ["OTHER_DEDUCTION"] = 0m
                };
                var snapshots = new List<PayrollComponentSnapshot>();
                decimal earnings = 0, deductions = 0;
                foreach (var component in components.Where(x => x.IsActive))
                {
                    var basis = values.TryGetValue(component.SalaryField, out var source) ? source : 0m;
                    var amount = component.CalculationType == "Percentage"
                        ? basis * component.Value / 100m
                        : (string.IsNullOrWhiteSpace(component.SalaryField) ? component.Value : basis);
                    if (component.ComponentType == "Earning") earnings += amount; else deductions += amount;
                    snapshots.Add(new PayrollComponentSnapshot
                    {
                        Code = component.Code, Name = component.Name, ComponentType = component.ComponentType,
                        Amount = amount, ConfiguredValue = component.Value, CalculationType = component.CalculationType,
                        IsTaxable = component.IsTaxable, IsPensionable = component.IsPensionable, IsWpsIncluded = component.IsWpsIncluded
                    });
                }

                var payroll = new Payroll
                {
                    EmployeeId = emp.EmployeeId,
                    Month = startDate,
                    BasicSalary = (double)salary,
                    OT1Hours = ot1,
                    OT2Hours = ot2,
                    OTEarnings = otEarnings,
                    Deductions = (double)deductions,
                    NetSalary = (double)(earnings - deductions),
                    IsApproved = false
                };
                payroll.ComponentSnapshots = snapshots;
                payrolls.Add(payroll);
            }

            _context.Payrolls.AddRange(payrolls);
            await _context.SaveChangesAsync();

            return _mapper.Map<List<PayrollDto>>(payrolls);
        }

        private async Task<List<PayrollComponent>> EnsureDefaultComponentsAsync()
        {
            var components = await _context.PayrollComponents.OrderBy(x => x.Id).ToListAsync();
            if (components.Count > 0) return components;
            var defaults = new[]
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
            foreach (var item in defaults)
                _context.PayrollComponents.Add(new PayrollComponent { Code = item.Item1, Name = item.Item2, ComponentType = item.Item3, SalaryField = item.Item4 });
            await _context.SaveChangesAsync();
            return await _context.PayrollComponents.OrderBy(x => x.Id).ToListAsync();
        }

        public async Task ApprovePayrollAsync(int payrollId)
        {
            var payroll = await _context.Payrolls.FindAsync(payrollId);
            if (payroll != null)
            {
                payroll.IsApproved = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<byte[]> GeneratePayslipPdfAsync(int payrollId)
        {
            // Placeholder: You can use a PDF generator like iTextSharp or QuestPDF
            var payroll = await _context.Payrolls.Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == payrollId);
            if (payroll == null) return null;

            var payslipText = $"Payslip for {payroll.Employee.FirstName} {payroll.Employee.LastName}\nMonth: {payroll.Month:yyyy-MM}\nNet Salary: {payroll.NetSalary}";
            return System.Text.Encoding.UTF8.GetBytes(payslipText);
        }

        public async Task<byte[]> ExportPayrollToExcelAsync(int year, int month)
        {
            var payrolls = await _context.Payrolls
                .Where(p => p.Month.Year == year && p.Month.Month == month)
                .Include(p => p.Employee)
                .ToListAsync();

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Payroll");

            sheet.Cells[1, 1].Value = "Employee";
            sheet.Cells[1, 2].Value = "Net Salary";

            int row = 2;
            foreach (var p in payrolls)
            {
                sheet.Cells[row, 1].Value = p.Employee.FirstName + " " + p.Employee.LastName;
                sheet.Cells[row, 2].Value = p.NetSalary;
                row++;
            }

            return package.GetAsByteArray();
        }
    }

}
