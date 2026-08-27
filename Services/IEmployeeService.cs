using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IEmployeeService
    {
            // Basic list (legacy)
            Task<List<EmployeeDto>> GetAllAsync();

            // New: filtered & paginated list
            Task<(List<EmployeeDto> items, int totalCount)> GetFilteredAsync(EmployeeFilterDto filter);

            Task<EmployeeDto> GetByIdAsync(int id);
            Task<EmployeeProfileDto?> GetProfileAsync(int id);
            Task<EmployeeDto> CreateAsync(EmployeeDto dto);
            Task<EmployeeDto> UpdateAsync(int id, EmployeeDto dto);
            Task<bool> DeleteAsync(int id);

            // Duplicate detection
            Task<DuplicateCheckResultDto> DuplicateCheckAsync(DuplicateCheckDto dto);

            // Change status with history
            Task<bool> ChangeStatusAsync(int employeeId, HRSystem.API.Models.EmployeeStatus newStatus, DateTime effectiveDate, DateTime? lastWorkingDate, string reason, int? changedByUserId, int? supportingDocumentId = null);
        }
}
