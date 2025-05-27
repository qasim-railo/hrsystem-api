using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IEmployeeShiftService
    {
        Task<List<EmployeeShiftDto>> GetAllAsync();
        Task<EmployeeShiftDto> GetByIdAsync(int id);
        Task<EmployeeShiftDto> CreateAsync(EmployeeShiftDto dto);
        Task UpdateAsync(int id, EmployeeShiftDto dto);
        Task DeleteAsync(int id);
    }
}
