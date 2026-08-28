using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IReportService
    {
        Task<List<EmployeeReportDto>> GetEmployeeReportAsync(ReportFilterDto filter);
        Task<byte[]> ExportEmployeeReportAsync(ReportFilterDto filter);
        Task<List<SalaryReportDto>> GetSalaryReportAsync(int? companyId, int? employeeId, int? month);
        Task<List<LeaveReportDto>> GetLeaveReportAsync(int? companyId, int? employeeId);
        Task<List<AssetReportDto>> GetAssetReportAsync(int? companyId, int? employeeId);
        Task<List<LoanBalanceReportDto>> GetLoanReportAsync(int? companyId, int? employeeId);
    }
}
