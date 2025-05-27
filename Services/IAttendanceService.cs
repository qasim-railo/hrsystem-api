using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IAttendanceService
    {
        Task AddAsync(AttendanceDto dto);
        Task<List<AttendanceDto>> GetByEmployeeAsync(int employeeId);
        Task ImportFromExcelAsync(IFormFile file);
    }
}
