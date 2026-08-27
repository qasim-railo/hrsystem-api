using HRSystem.API.DTOs;
using HRSystem.API.Models;

namespace HRSystem.API.Services;

public interface IFileService
{
    Task<FileRecordDto> UploadAsync(FileUploadDto request, string uploadedBy, CancellationToken cancellationToken = default);
    Task<(Stream Content, FileRecord Record)?> OpenReadAsync(int fileId, CancellationToken cancellationToken = default);
    Task<FileRecordDto?> GetAsync(int fileId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int fileId, CancellationToken cancellationToken = default);
}
