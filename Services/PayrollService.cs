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

            var employees = await _context.Employees.ToListAsync();
            var payrolls = new List<Payroll>();

            foreach (var emp in employees)
            {
                var attendance = await _context.Attendance
                    .Where(a => a.EmployeeId == emp.EmployeeId && a.Date >= startDate && a.Date <= endDate)
                    .ToListAsync();

                var ot1 = attendance.Sum(a => a.OT1);
                var ot2 = attendance.Sum(a => a.OT2);
                var otEarnings = (ot1 * 20) + (ot2 * 30); // example rates

                var payroll = new Payroll
                {
                    EmployeeId = emp.EmployeeId,
                    Month = startDate,
                    BasicSalary = 2000, // Replace with actual salary
                    OT1Hours = ot1,
                    OT2Hours = ot2,
                    OTEarnings = otEarnings,
                    Deductions = 0, // calculate if needed
                    NetSalary = 2000 + otEarnings,
                    IsApproved = false
                };

                payrolls.Add(payroll);
            }

            _context.Payrolls.AddRange(payrolls);
            await _context.SaveChangesAsync();

            return _mapper.Map<List<PayrollDto>>(payrolls);
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
