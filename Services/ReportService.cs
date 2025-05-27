using HRSystem.API.Data;
using HRSystem.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalaryReportDto>> GetSalaryReportAsync(int? companyId, int? employeeId, int? month)
        {
            var query = _context.Payrolls.Include(p => p.Employee).ThenInclude(e => e.Company).AsQueryable();

            if (companyId.HasValue)
                query = query.Where(p => p.Employee.CompanyId == companyId.Value);

            if (employeeId.HasValue)
                query = query.Where(p => p.EmployeeId == employeeId.Value);

            if (month.HasValue)
                query = query.Where(p => p.Month.Month == month.Value);

            return await query.Select(p => new SalaryReportDto
            {
                EmployeeId = p.EmployeeId,
                EmployeeName = p.Employee.FirstName + ' ' + p.Employee.LastName,
                CompanyName = p.Employee.Company.Name,
                Month = p.Month,
                BasicSalary = (decimal)p.BasicSalary,
                OT1Hours = p.OT1Hours,
                OT2Hours = p.OT2Hours,
                NetSalary = (decimal)p.NetSalary
            }).ToListAsync();
        }

        public async Task<List<LeaveReportDto>> GetLeaveReportAsync(int? companyId, int? employeeId)
        {
            var query = _context.LeaveRequests.Include(l => l.Employee).AsQueryable();

            if (companyId.HasValue)
                query = query.Where(l => l.Employee.CompanyId == companyId.Value);

            if (employeeId.HasValue)
                query = query.Where(l => l.EmployeeId == employeeId.Value);

            return await query
                .GroupBy(l => new { l.EmployeeId, l.Employee.FirstName , l.Employee.LastName })
                .Select(g => new LeaveReportDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    EmployeeName = g.Key.FirstName+' '+g.Key.LastName,
                    TotalLeaves = g.Count(),
                    ApprovedLeaves = g.Count(x => x.Status == "Approved"),
                    PendingLeaves = g.Count(x => x.Status == "Pending")
                }).ToListAsync();
        }

        public async Task<List<AssetReportDto>> GetAssetReportAsync(int? companyId, int? employeeId)
        {
            var query = _context.EmployeeAssets
                .Include(ea => ea.Employee)
                .Include(ea => ea.Asset)
                .AsQueryable();

            if (companyId.HasValue)
                query = query.Where(ea => ea.Employee.CompanyId == companyId.Value);

            if (employeeId.HasValue)
                query = query.Where(ea => ea.EmployeeId == employeeId.Value);

            return await query.Select(ea => new AssetReportDto
            {
                EmployeeId = ea.EmployeeId,
                EmployeeName = ea.Employee.FirstName+' ' + ea.Employee.LastName,
                AssetName = ea.Asset.Name,
                AssetCode = ea.Asset.AssetCode,
                AssignedDate = ea.AssignedDate,
                Status = ea.Status
            }).ToListAsync();
        }

        Task<List<LoanBalanceReportDto>> IReportService.GetLoanReportAsync(int? companyId, int? employeeId)
        {
            throw new NotImplementedException();
        }

        //public async Task<List<LoanBalanceReportDto>> GetLoanReportAsync(int? companyId, int? employeeId)
        //{

        //    var query = _context.Loans.Include(l => l.Employee).AsQueryable();

        //    if (companyId.HasValue)
        //        query = query.Where(l => l.Employee.CompanyId == companyId.Value);

        //    if (employeeId.HasValue)
        //        query = query.Where(l => l.EmployeeId == employeeId.Value);

        //    return await query.Select(l => new LoanBalanceReportDto
        //    {
        //        EmployeeId = l.EmployeeId,
        //        EmployeeName = l.Employee.FullName,
        //        TotalLoanAmount = l.Amount,
        //        PaidAmount = l.PaidAmount,
        //        RemainingBalance = l.Amount - l.PaidAmount
        //    }).ToListAsync();
        //}
    }

}
