using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IEmployeeAssetsService
    {
        Task<List<EmployeeAssetDto>> GetAllAsync();
        Task<EmployeeAssetDto> GetByIdAsync(int id);
        Task<EmployeeAssetDto> AssignAsync(EmployeeAssetDto dto);
        Task<EmployeeAssetDto> UpdateAsync(int id, EmployeeAssetDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
