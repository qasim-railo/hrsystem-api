using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class GratuityReportService : IGratuityReportService
    {
        private readonly AppDbContext _context;

        public GratuityReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GratuityReportDto> GenerateAsync(int employeeId, DateTime endDate)
        {
            var employee = await _context.Employees
                .Include(e => e.EmploymentDetail)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null || employee.EmploymentDetail == null)
                throw new Exception("Employee or Employment Detail not found");

            var joinDate = employee.EmploymentDetail.JoiningDate;
            var basicSalary = employee.EmploymentDetail.BasicSalary;

            var yearsWorked = (endDate - joinDate).TotalDays / 365.0;
            double gratuityAmount = 0.0;

            if (yearsWorked >= 1)
            {
                // Sample logic: 21 days per year of basic salary
               // gratuityAmount = (21.0 / 30.0) * basicSalary * yearsWorked;
                gratuityAmount = (double)((21m / 30m) * basicSalary * (decimal)yearsWorked);
            }

            return new GratuityReportDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                JoinDate = joinDate,
                EndDate = endDate,
                BasicSalary = (double)basicSalary,
                GratuityAmount = Math.Round(gratuityAmount, 2)
            };
        }
    }

}
