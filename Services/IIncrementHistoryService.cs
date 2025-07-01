using HRSystem.API.DTOs; 
namespace HRSystem.API.Services
{
    public interface IIncrementHistoryService
    {
        Task AddAsync(IncrementHistoryDto dto);
        Task<List<IncrementHistoryDto>> GetByEmployeeAsync(int employeeId);
        Task<List<IncrementHistoryDto>> GetAllAsync();
    }
}
