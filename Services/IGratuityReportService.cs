using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IGratuityReportService
    {
        Task<GratuityReportDto> GenerateAsync(int employeeId, DateTime endDate);
    }
}
