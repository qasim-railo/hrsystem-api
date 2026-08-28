using HRSystem.API.Data;
using HRSystem.API.DTOs;
using OfficeOpenXml;
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

        public async Task<List<EmployeeReportDto>> GetEmployeeReportAsync(ReportFilterDto filter)
        {
            var query = _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Department)
                .Include(e => e.Branch)
                .Include(e => e.EmploymentDetail)
                .AsQueryable();

            if (filter.CompanyId.HasValue) query = query.Where(e => e.CompanyId == filter.CompanyId.Value);
            if (filter.BranchId.HasValue) query = query.Where(e => e.BranchId == filter.BranchId.Value);
            if (filter.DepartmentId.HasValue) query = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);
            if (filter.EmployeeId.HasValue) query = query.Where(e => e.EmployeeId == filter.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Status)) query = query.Where(e => e.Status.ToString() == filter.Status);
            if (filter.FromDate.HasValue) query = query.Where(e => e.EmploymentDetail != null && e.EmploymentDetail.JoiningDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue) query = query.Where(e => e.EmploymentDetail != null && e.EmploymentDetail.JoiningDate <= filter.ToDate.Value);

            return await query.OrderBy(e => e.EmployeeCode).Select(e => new EmployeeReportDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                EmployeeName = e.FirstName + " " + e.LastName,
                CompanyName = e.Company.Name,
                DepartmentName = e.Department.Name,
                BranchName = e.Branch == null ? string.Empty : e.Branch.Name,
                Status = e.Status.ToString(),
                DateOfJoining = e.EmploymentDetail == null ? null : e.EmploymentDetail.JoiningDate
            }).ToListAsync();
        }

        public async Task<byte[]> ExportEmployeeReportAsync(ReportFilterDto filter)
        {
            var rows = await GetEmployeeReportAsync(filter);
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Employees");
            var headers = new[] { "Employee Code", "Employee", "Company", "Department", "Branch", "Status", "Date of Joining" };
            for (var i = 0; i < headers.Length; i++) sheet.Cells[1, i + 1].Value = headers[i];
            for (var row = 0; row < rows.Count; row++)
            {
                var item = rows[row];
                var line = row + 2;
                sheet.Cells[line, 1].Value = item.EmployeeCode;
                sheet.Cells[line, 2].Value = item.EmployeeName;
                sheet.Cells[line, 3].Value = item.CompanyName;
                sheet.Cells[line, 4].Value = item.DepartmentName;
                sheet.Cells[line, 5].Value = item.BranchName;
                sheet.Cells[line, 6].Value = item.Status;
                sheet.Cells[line, 7].Value = item.DateOfJoining;
            }
            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
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
