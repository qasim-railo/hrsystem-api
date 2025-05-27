using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface ICompaniesService
    {
        Task<List<CompanyDto>> GetAllAsync();
        Task<CompanyDto> GetByIdAsync(int id);
        Task<CompanyDto> CreateAsync(CompanyDto dto);
        Task<CompanyDto> UpdateAsync(int id, CompanyDto dto);
        Task<bool> DeleteAsync(int id);
    }

}
