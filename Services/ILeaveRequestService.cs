using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequestResponseDto>> GetAllAsync();
        Task<LeaveRequestResponseDto> GetByIdAsync(int id);
        Task AddAsync(LeaveRequestDto dto);
        Task ApproveAsync(int id, int approvedBy);
        Task RejectAsync(int id);
    }
}
