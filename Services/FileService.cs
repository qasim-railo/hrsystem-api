using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.EntityFrameworkCore;

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
        if (request.EntityType.Equals("Employee", StringComparison.OrdinalIgnoreCase) &&
            (!int.TryParse(request.EntityId, out var employeeId) ||
             !await _db.Employees.AnyAsync(x => x.EmployeeId == employeeId, cancellationToken)))
            throw new InvalidOperationException("The employee does not exist in the current tenant.");
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
            EntityType = request.EntityType.Trim(),
            EntityId = request.EntityId.Trim(),
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
