using Microsoft.Extensions.Options;

namespace HRSystem.API.Services;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IWebHostEnvironment environment, IOptions<FileStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public async Task<StoredFile> UploadAsync(Stream content, int tenantId, string extension, CancellationToken cancellationToken = default)
    {
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var storagePath = Path.Combine(tenantId.ToString(), storedFileName);
        var fullPath = GetFullPath(storagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        try
        {
            await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }
        return new StoredFile(storedFileName, storagePath);
    }

    public Task<Stream?> DownloadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        Stream? result = File.Exists(fullPath)
            ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read)
            : null;
        return Task.FromResult(result);
    }

    public Task<bool> DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(storagePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.FromResult(!File.Exists(fullPath));
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
        => Task.FromResult(File.Exists(GetFullPath(storagePath)));

    public Task<string?> GetSecureDownloadReferenceAsync(string storagePath, TimeSpan lifetime, CancellationToken cancellationToken = default)
        => Task.FromResult<string?>(null);

    private string GetFullPath(string storagePath)
    {
        var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));
        var path = Path.GetFullPath(Path.Combine(root, storagePath));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        return path;
    }
}
