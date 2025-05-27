using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IPayrollService
    {
        Task<List<PayrollDto>> GenerateMonthlyPayrollAsync(int year, int month);
        Task ApprovePayrollAsync(int payrollId);
        Task<byte[]> GeneratePayslipPdfAsync(int payrollId);
        Task<byte[]> ExportPayrollToExcelAsync(int year, int month);
    }
}
