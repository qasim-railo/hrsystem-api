using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IAssetsService
    {
        Task<List<AssetDto>> GetAllAsync();
        Task<AssetDto> GetByIdAsync(int id);
        Task<AssetDto> CreateAsync(AssetDto dto);
        Task<AssetDto> UpdateAsync(int id, AssetDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
