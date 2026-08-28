using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HRSystem.API.Services;

public sealed class FileService : IFileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentTenant _currentTenant;
    private readonly FileStorageOptions _options;
    private readonly IMalwareScanner _malwareScanner;

    public FileService(AppDbContext db, IWebHostEnvironment environment, ICurrentTenant currentTenant,
        IOptions<FileStorageOptions>? options = null, IMalwareScanner? malwareScanner = null)
    {
        _db = db;
        _environment = environment;
        _currentTenant = currentTenant;
        _options = options?.Value ?? new FileStorageOptions();
        _malwareScanner = malwareScanner ?? new NoOpMalwareScanner();
    }

    public async Task<FileRecordDto> UploadAsync(FileUploadDto request, string uploadedBy, CancellationToken cancellationToken = default)
    {
        await EnsureQuotaAsync(request.File?.Length ?? 0, cancellationToken);
        var stored = await ValidateAndStoreAsync(request.File, request.EntityType, request.EntityId, cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var record = new FileRecord
            {
                TenantId = stored.TenantId,
                EntityType = request.EntityType.Trim(), EntityId = request.EntityId.Trim(),
                DocumentType = request.DocumentType?.Trim() ?? string.Empty,
                OriginalFileName = stored.OriginalName, StoredFileName = stored.StoredName,
                StoragePath = stored.RelativePath, MimeType = stored.MimeType, Size = request.File.Length,
                Extension = stored.Extension, UploadedBy = uploadedBy ?? string.Empty,
                UploadedAt = now, UpdatedAt = now
            };
            _db.FileRecords.Add(record);
            await _db.SaveChangesAsync(cancellationToken);
            await ReconcileStorageUsageAsync(cancellationToken);
            return ToDto(record);
        }
        catch
        {
            try { if (File.Exists(stored.FullPath)) File.Delete(stored.FullPath); }
            catch { /* retain the original failure; cleanup can be retried by an operator */ }
            throw;
        }
    }

    public async Task<FileRecordDto?> ReplaceAsync(int fileId, IFormFile file, string uploadedBy, CancellationToken cancellationToken = default)
    {
        var current = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId, cancellationToken);
        if (current is null || !current.IsCurrent || current.Status != "Active") return null;
        await EnsureQuotaAsync((file?.Length ?? 0) - current.Size, cancellationToken);
        var stored = await ValidateAndStoreAsync(file, current.EntityType, current.EntityId, cancellationToken);
        await using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var versions = await _db.FileRecords
                .Where(x => x.EntityType == current.EntityType && x.EntityId == current.EntityId &&
                            x.DocumentType == current.DocumentType)
                .ToListAsync(cancellationToken);
            var latest = versions.Max(x => x.Version);
            foreach (var version in versions.Where(x => x.IsCurrent))
            {
                version.IsCurrent = false;
                version.Status = "Inactive";
                version.UpdatedAt = now;
            }
            var replacement = new FileRecord
            {
                TenantId = current.TenantId, EntityType = current.EntityType, EntityId = current.EntityId,
                DocumentType = current.DocumentType, OriginalFileName = stored.OriginalName,
                StoredFileName = stored.StoredName, StoragePath = stored.RelativePath, MimeType = stored.MimeType,
                Size = file.Length, Extension = stored.Extension, UploadedBy = uploadedBy ?? string.Empty,
                UploadedAt = now, UpdatedAt = now, Version = latest + 1, Status = "Active", IsCurrent = true
            };
            _db.FileRecords.Add(replacement);
            await _db.SaveChangesAsync(cancellationToken);
            await ReconcileStorageUsageAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToDto(replacement);
        }
        catch
        {
            try { if (File.Exists(stored.FullPath)) File.Delete(stored.FullPath); }
            catch { }
            throw;
        }
    }

    public async Task<IReadOnlyList<FileRecordDto>> GetVersionHistoryAsync(int fileId, CancellationToken cancellationToken = default)
    {
        var file = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId, cancellationToken);
        if (file is null) return [];
        return await _db.FileRecords.AsNoTracking()
            .Where(x => x.EntityType == file.EntityType && x.EntityId == file.EntityId && x.DocumentType == file.DocumentType)
            .OrderByDescending(x => x.Version).Select(x => new FileRecordDto
            {
                FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
                DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
                Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
                UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status, IsCurrent = x.IsCurrent
            }).ToListAsync(cancellationToken);
    }

    public async Task<(Stream Content, FileRecord Record)?> OpenReadAsync(int fileId, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.Status == "Active", cancellationToken);
        if (record is null) return null;
        var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));
        var path = Path.GetFullPath(Path.Combine(root, record.StoragePath));
        if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read), record);
    }

    public async Task<FileRecordDto?> GetAsync(int fileId, CancellationToken cancellationToken = default)
        => await _db.FileRecords.Where(x => x.FileId == fileId).Select(x => new FileRecordDto
        {
            FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
            DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
            Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
            UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status, IsCurrent = x.IsCurrent
        }).SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<FileRecordDto>> SearchAsync(FileSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _db.FileRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(request.EntityType))
           query = query.Where(x => x.EntityType == request.EntityType.Trim());
        if (!string.IsNullOrWhiteSpace(request.EntityId))
           query = query.Where(x => x.EntityId == request.EntityId.Trim());
        if (!string.IsNullOrWhiteSpace(request.DocumentType))
           query = query.Where(x => x.DocumentType == request.DocumentType.Trim());
        if (!string.IsNullOrWhiteSpace(request.Status))
           query = query.Where(x => x.Status == request.Status.Trim());
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
           var search = request.Search.Trim();
           query = query.Where(x => x.OriginalFileName.Contains(search) ||
               x.DocumentType.Contains(search) || x.EntityType.Contains(search) ||
               x.EntityId.Contains(search) || x.UploadedBy.Contains(search));
        }
        if (request.FromDate is DateTime from)
           query = query.Where(x => x.UploadedAt >= from);
        if (request.ToDate is DateTime to)
           query = query.Where(x => x.UploadedAt <= to);

        return await query.OrderByDescending(x => x.UploadedAt).Take(500)
           .Select(x => new FileRecordDto
           {
               FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
               DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
               Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
               UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status, IsCurrent = x.IsCurrent
           }).ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int fileId, string deletedBy, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId, cancellationToken);
        if (record is null) return false;
        var now = DateTime.UtcNow;
        var versions = await _db.FileRecords.Where(x => x.EntityType == record.EntityType &&
            x.EntityId == record.EntityId && x.DocumentType == record.DocumentType).ToListAsync(cancellationToken);
        foreach (var version in versions)
        {
            version.Status = "Deleted";
            version.IsCurrent = false;
            version.IsDeleted = true;
            version.DeletedAt = now;
            version.DeletedBy = deletedBy;
            version.UpdatedAt = now;
        }
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = record.TenantId, Action = "FileDeleted", Entity = "FileRecord",
            EntityId = record.FileId.ToString(), UserId = deletedBy, CreatedAt = now,
            Details = $"Moved {versions.Count} file version(s) to the recycle bin."
        });
        await _db.SaveChangesAsync(cancellationToken);
        await ReconcileStorageUsageAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<FileRecordDto>> GetRecycleBinAsync(CancellationToken cancellationToken = default)
        => await _db.FileRecords.AsNoTracking().Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt).Take(500).Select(ToDtoExpression()).ToListAsync(cancellationToken);

    public async Task<FileRecordDto?> RestoreAsync(int fileId, string restoredBy, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.IsDeleted, cancellationToken);
        if (record is null) return null;
        var versions = await _db.FileRecords.Where(x => x.EntityType == record.EntityType &&
            x.EntityId == record.EntityId && x.DocumentType == record.DocumentType).ToListAsync(cancellationToken);
        var restored = versions.OrderByDescending(x => x.Version).ThenByDescending(x => x.FileId).First();
        var now = DateTime.UtcNow;
        foreach (var version in versions)
        {
            version.IsDeleted = false;
            version.DeletedAt = null;
            version.DeletedBy = null;
            version.IsCurrent = version.FileId == restored.FileId;
            version.Status = version.IsCurrent ? "Active" : "Inactive";
            version.UpdatedAt = now;
        }
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = record.TenantId, Action = "FileRestored", Entity = "FileRecord",
            EntityId = record.FileId.ToString(), UserId = restoredBy, CreatedAt = now,
            Details = $"Restored version {restored.Version}."
        });
        await _db.SaveChangesAsync(cancellationToken);
        await ReconcileStorageUsageAsync(cancellationToken);
        return ToDto(restored);
    }

    public async Task<bool> PurgeAsync(int fileId, string purgedBy, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.IsDeleted, cancellationToken);
        if (record is null) return false;
        var versions = await _db.FileRecords.Where(x => x.EntityType == record.EntityType &&
            x.EntityId == record.EntityId && x.DocumentType == record.DocumentType).ToListAsync(cancellationToken);
        var paths = versions.Select(x => GetFullPath(x.StoragePath)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        foreach (var path in paths)
        {
            if (File.Exists(path) && !TryDeletePhysicalFile(path))
                throw new IOException($"Unable to permanently delete stored file '{Path.GetFileName(path)}'.");
        }
        _db.FileRecords.RemoveRange(versions);
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = record.TenantId, Action = "FilePurged", Entity = "FileRecord",
            EntityId = record.FileId.ToString(), UserId = purgedBy, CreatedAt = DateTime.UtcNow,
            Details = $"Permanently purged {versions.Count} file version(s)."
        });
        await _db.SaveChangesAsync(cancellationToken);
        await ReconcileStorageUsageAsync(cancellationToken);
        return true;
    }

    public async Task<StorageQuotaDto> GetStorageQuotaAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTenant.TenantId is not int tenantId) throw new UnauthorizedAccessException("A tenant context is required.");
        var tenant = await _db.Tenants.Include(x => x.Plan).SingleAsync(x => x.TenantId == tenantId, cancellationToken);
        var used = await GetActualUsageAsync(tenantId, cancellationToken);
        return ToQuota(used, tenant.Plan?.MaxStorageBytes ?? 0);
    }

    public async Task<StorageQuotaDto> ReconcileStorageUsageAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTenant.TenantId is not int tenantId) throw new UnauthorizedAccessException("A tenant context is required.");
        var tenant = await _db.Tenants.Include(x => x.Plan).SingleAsync(x => x.TenantId == tenantId, cancellationToken);
        tenant.StorageUsedBytes = await GetActualUsageAsync(tenantId, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return ToQuota(tenant.StorageUsedBytes, tenant.Plan?.MaxStorageBytes ?? 0);
    }

    private static FileRecordDto ToDto(FileRecord x) => new()
    {
        FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
        DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
        Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
        UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status, IsCurrent = x.IsCurrent,
        IsDeleted = x.IsDeleted, DeletedAt = x.DeletedAt, DeletedBy = x.DeletedBy
    };

    private async Task EnsureQuotaAsync(long incomingBytes, CancellationToken cancellationToken)
    {
        if (incomingBytes <= 0) return;
        if (_currentTenant.TenantId is not int tenantId) throw new UnauthorizedAccessException("A tenant context is required.");
        var tenant = await _db.Tenants.Include(x => x.Plan).SingleAsync(x => x.TenantId == tenantId, cancellationToken);
        var used = await GetActualUsageAsync(tenantId, cancellationToken);
        var limit = tenant.Plan?.MaxStorageBytes ?? 0;
        if (limit > 0 && used + incomingBytes > limit)
            throw new InvalidOperationException($"Storage quota exceeded. {limit - used:N0} bytes remain.");
    }

    private async Task<long> GetActualUsageAsync(int tenantId, CancellationToken cancellationToken)
        => await _db.FileRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.IsCurrent && !x.IsDeleted && x.Status == "Active")
            .SumAsync(x => (long?)x.Size, cancellationToken) ?? 0L;

    private static StorageQuotaDto ToQuota(long used, long limit) => new()
    {
        UsedBytes = used,
        LimitBytes = limit,
        RemainingBytes = limit > 0 ? Math.Max(0, limit - used) : long.MaxValue,
        UsagePercent = limit > 0 ? Math.Round((double)used / limit * 100, 2) : 0
    };

    private static System.Linq.Expressions.Expression<Func<FileRecord, FileRecordDto>> ToDtoExpression() => x => new FileRecordDto
    {
       FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
       DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
       Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
       UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status, IsCurrent = x.IsCurrent,
       IsDeleted = x.IsDeleted, DeletedAt = x.DeletedAt, DeletedBy = x.DeletedBy
    };

    private string GetFullPath(string relativePath)
    {
       var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));
       var path = Path.GetFullPath(Path.Combine(root, relativePath));
       if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
           throw new InvalidOperationException("Invalid storage path.");
       return path;
    }

    private static bool TryDeletePhysicalFile(string path)
    {
       try { File.Delete(path); return !File.Exists(path); }
       catch { return false; }
    }

    private async Task<(int TenantId, string OriginalName, string Extension, string MimeType, string StoredName, string RelativePath, string FullPath)>
        ValidateAndStoreAsync(IFormFile file, string entityTypeInput, string entityIdInput, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) throw new InvalidOperationException("A non-empty file is required.");
        var entityType = entityTypeInput.Trim();
        var entityId = entityIdInput.Trim();
        if (string.IsNullOrWhiteSpace(entityType) || string.IsNullOrWhiteSpace(entityId))
            throw new InvalidOperationException("EntityType and EntityId are required.");
        if (entityType.Length > 128 || entityId.Length > 128 ||
            !Regex.IsMatch(entityType, "^[A-Za-z][A-Za-z0-9_.-]*$") ||
            !Regex.IsMatch(entityId, "^[A-Za-z0-9][A-Za-z0-9_.:/-]*$"))
            throw new InvalidOperationException("EntityType or EntityId has an invalid format.");
        if (_options.MaxFileSizeBytes <= 0 || file.Length > _options.MaxFileSizeBytes)
            throw new InvalidOperationException($"The file exceeds the {_options.MaxFileSizeBytes / (1024 * 1024)} MB limit.");
        var originalName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalName) || originalName != file.FileName ||
            originalName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || originalName.Any(char.IsControl))
            throw new InvalidOperationException("The file name is invalid.");
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        var mimeType = file.ContentType?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) ||
            !new HashSet<string>(_options.AllowedExtensions ?? [], StringComparer.OrdinalIgnoreCase).Contains(extension))
            throw new InvalidOperationException("This file type is not allowed.");
        if (string.IsNullOrEmpty(mimeType) || !_options.AllowedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase) ||
            !MimeMatchesExtension(extension, mimeType))
            throw new InvalidOperationException("The file MIME type is not allowed for its extension.");
        if (_currentTenant.TenantId is not int tenantId || tenantId <= 0)
            throw new UnauthorizedAccessException("A tenant context is required.");
        var storedName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(tenantId.ToString(), storedName);
        var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, _options.RootPath));
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using (var scanStream = file.OpenReadStream())
            await _malwareScanner.ScanAsync(scanStream, originalName, cancellationToken);
        try
        {
            await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await file.CopyToAsync(output, cancellationToken);
        }
        catch
        {
            try { if (File.Exists(fullPath)) File.Delete(fullPath); } catch { }
            throw;
        }
        return (tenantId, originalName, extension, mimeType, storedName, relativePath, fullPath);
    }

    private static bool MimeMatchesExtension(string extension, string mimeType) => extension switch
    {
        ".pdf" => mimeType == "application/pdf",
        ".png" => mimeType == "image/png",
        ".jpg" or ".jpeg" => mimeType == "image/jpeg",
        ".doc" => mimeType == "application/msword",
        ".docx" => mimeType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".xls" => mimeType == "application/vnd.ms-excel",
        ".xlsx" => mimeType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".txt" => mimeType == "text/plain",
        _ => false
    };
}
