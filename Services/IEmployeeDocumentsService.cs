using HRSystem.API.DTOs;

namespace HRSystem.API.Services
{
    public interface IEmployeeDocumentsService
    {
        Task<string> UploadAsync(EmployeeDocumentUploadDto dto);
        Task<List<EmployeeDocumentDto>> GetByEmployeeIdAsync(int employeeId);
        Task<(byte[] FileBytes, string FileName)> DownloadAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
