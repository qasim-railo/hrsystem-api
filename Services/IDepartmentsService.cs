using HRSystem.API.Dtos;

namespace HRSystem.API.Services.Interfaces
{
    public interface IDepartmentsService
    {
        Task<List<DepartmentDto>> GetAllAsync();
        Task<DepartmentDto?> GetByIdAsync(int id);
        Task<IEnumerable<DepartmentDto>> GetByCompanyAsync(int companyId);

        Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto);
        Task<DepartmentDto?> UpdateAsync(int id, UpdateDepartmentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
