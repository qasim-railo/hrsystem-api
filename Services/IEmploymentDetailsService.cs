
using HRSystem.API.DTOs; 

namespace HRSystem.API.Services
{
    public interface IEmploymentDetailsService
    {
        Task<EmploymentDetailDto> GetByEmployeeIdAsync(int employeeId);
        Task<EmploymentDetailDto?> GetByIdAsync(int id);
        Task<EmploymentDetailDto> CreateAsync(CreateEmploymentDetailDto dto);
        Task<EmploymentDetailDto?> UpdateAsync(int id, UpdateEmploymentDetailDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
