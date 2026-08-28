namespace HRSystem.API.Services;

public sealed record StoredFile(string StoredFileName, string StoragePath);

public interface IFileStorageService
{
    Task<StoredFile> UploadAsync(Stream content, int tenantId, string extension, CancellationToken cancellationToken = default);
    Task<Stream?> DownloadAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default);
    Task<string?> GetSecureDownloadReferenceAsync(string storagePath, TimeSpan lifetime, CancellationToken cancellationToken = default);
}
