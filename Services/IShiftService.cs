using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IShiftService
    {
        Task<List<ShiftDto>> GetAllAsync();
        Task<ShiftDto> GetByIdAsync(int id);
        Task<ShiftDto> CreateAsync(ShiftDto dto);
        Task UpdateAsync(int id, ShiftDto dto);
        Task DeleteAsync(int id);
    }
}
