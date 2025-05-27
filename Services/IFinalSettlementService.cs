using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IFinalSettlementService
    {
        Task<List<FinalSettlementDto>> GetAllAsync();
        Task<FinalSettlementDto> GetByIdAsync(int id);
        Task GenerateSettlementAsync(int employeeId);
        Task SignOffAsync(int settlementId, bool isEmployeeSign, bool isHRSign);
    }

}
