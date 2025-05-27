using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto>> GetAllAsync();
        Task<EmployeeDto> GetByIdAsync(int id);
        Task<EmployeeDto> CreateAsync(EmployeeDto dto);
        Task<EmployeeDto> UpdateAsync(int id, EmployeeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
