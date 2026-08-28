using HRSystem.API.DTOs;
using HRSystem.API.Models;

namespace HRSystem.API.Services;

public interface IFileService
{
    Task<FileRecordDto> UploadAsync(FileUploadDto request, string uploadedBy, CancellationToken cancellationToken = default);
    Task<(Stream Content, FileRecord Record)?> OpenReadAsync(int fileId, CancellationToken cancellationToken = default);
    Task<FileRecordDto?> GetAsync(int fileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecordDto>> SearchAsync(FileSearchRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int fileId, string deletedBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecordDto>> GetRecycleBinAsync(CancellationToken cancellationToken = default);
    Task<FileRecordDto?> RestoreAsync(int fileId, string restoredBy, CancellationToken cancellationToken = default);
    Task<bool> PurgeAsync(int fileId, string purgedBy, CancellationToken cancellationToken = default);
    Task<StorageQuotaDto> GetStorageQuotaAsync(CancellationToken cancellationToken = default);
    Task<StorageQuotaDto> ReconcileStorageUsageAsync(CancellationToken cancellationToken = default);
    Task<FileRecordDto?> ReplaceAsync(int fileId, IFormFile file, string uploadedBy, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FileRecordDto>> GetVersionHistoryAsync(int fileId, CancellationToken cancellationToken = default);
}
