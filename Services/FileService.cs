using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HRSystem.API.Services;

public sealed class FileService : IFileService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentTenant _currentTenant;

    public FileService(AppDbContext db, IWebHostEnvironment environment, ICurrentTenant currentTenant)
    {
        _db = db;
        _environment = environment;
        _currentTenant = currentTenant;
    }

    public async Task<FileRecordDto> UploadAsync(FileUploadDto request, string uploadedBy, CancellationToken cancellationToken = default)
    {
        if (request.File is null || request.File.Length == 0) throw new InvalidOperationException("A non-empty file is required.");
        if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.EntityId))
            throw new InvalidOperationException("EntityType and EntityId are required.");
        var entityType = request.EntityType.Trim();
        var entityId = request.EntityId.Trim();
        if (entityType.Length > 128 || entityId.Length > 128 ||
            !Regex.IsMatch(entityType, "^[A-Za-z][A-Za-z0-9_.-]*$") ||
            !Regex.IsMatch(entityId, "^[A-Za-z0-9][A-Za-z0-9_.:/-]*$"))
            throw new InvalidOperationException("EntityType or EntityId has an invalid format.");
        if (request.File.Length > 25 * 1024 * 1024) throw new InvalidOperationException("The file exceeds the 25 MB limit.");

        var extension = Path.GetExtension(request.File.FileName);
        if (!AllowedExtensions.Contains(extension)) throw new InvalidOperationException("This file type is not allowed.");
        if (_currentTenant.TenantId is not int parsedTenant || parsedTenant <= 0)
            throw new UnauthorizedAccessException("A tenant context is required.");

        var storedName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var relativePath = Path.Combine(parsedTenant.ToString(), storedName);
        var root = Path.Combine(_environment.ContentRootPath, "PrivateStorage");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(root) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid storage path.");
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using (var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            await request.File.CopyToAsync(output, cancellationToken);

        var now = DateTime.UtcNow;
        var record = new FileRecord
        {
            EntityType = entityType,
            EntityId = entityId,
            DocumentType = request.DocumentType?.Trim() ?? string.Empty,
            OriginalFileName = Path.GetFileName(request.File.FileName),
            StoredFileName = storedName,
            StoragePath = relativePath,
            MimeType = string.IsNullOrWhiteSpace(request.File.ContentType) ? "application/octet-stream" : request.File.ContentType,
            Size = request.File.Length,
            Extension = extension.ToLowerInvariant(),
            UploadedBy = uploadedBy ?? string.Empty,
            UploadedAt = now,
            UpdatedAt = now
        };
        _db.FileRecords.Add(record);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(record);
    }

    public async Task<(Stream Content, FileRecord Record)?> OpenReadAsync(int fileId, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId && x.Status == "Active", cancellationToken);
        if (record is null) return null;
        var root = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "PrivateStorage"));
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
            UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status
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
               UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status
           }).ToListAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int fileId, CancellationToken cancellationToken = default)
    {
        var record = await _db.FileRecords.SingleOrDefaultAsync(x => x.FileId == fileId, cancellationToken);
        if (record is null) return false;
        record.Status = "Deleted";
        record.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static FileRecordDto ToDto(FileRecord x) => new()
    {
        FileId = x.FileId, TenantId = x.TenantId, EntityType = x.EntityType, EntityId = x.EntityId,
        DocumentType = x.DocumentType, OriginalFileName = x.OriginalFileName, MimeType = x.MimeType,
        Size = x.Size, Extension = x.Extension, UploadedBy = x.UploadedBy, UploadedAt = x.UploadedAt,
        UpdatedAt = x.UpdatedAt, Version = x.Version, Status = x.Status
    };
}
