using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmployeeDocumentsService : IEmployeeDocumentsService
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public EmployeeDocumentsService(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<string> UploadAsync(EmployeeDocumentUploadDto dto)
        {
            var file = await _fileService.UploadAsync(new FileUploadDto
            {
                EntityType = "Employee",
                EntityId = dto.EmployeeId.ToString(),
                DocumentType = dto.FileType ?? "Employee document",
                File = dto.File
            }, "employee-upload");

            var document = new EmployeeDocument
            {
                EmployeeId = dto.EmployeeId,
                FileName = dto.File.FileName,
                FilePath = $"api/files/{file.FileId}/download",
                UploadedAt = DateTime.UtcNow,
                FileType = dto.FileType
            };

            _context.EmployeeDocuments.Add(document);
            await _context.SaveChangesAsync();

            return document.FilePath;
        }

        public async Task<List<EmployeeDocumentDto>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeeDocuments
                .Where(d => d.EmployeeId == employeeId)
                .Select(d => new EmployeeDocumentDto
                {
                    Id = d.Id,
                    EmployeeId = d.EmployeeId,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    UploadedAt = d.UploadedAt,
                    FileType = d.FileType
                }).ToListAsync();
        }

        public async Task<(byte[] FileBytes, string FileName)> DownloadAsync(int id)
        {
            var doc = await _context.EmployeeDocuments.FindAsync(id);
            if (doc == null) return (null, null);
            var marker = "api/files/";
            var start = doc.FilePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0 || !int.TryParse(doc.FilePath[(start + marker.Length)..].Split('/')[0], out var fileId))
                return (null, null);
            var result = await _fileService.OpenReadAsync(fileId);
            if (result is null) return (null, null);
            await using var stream = result.Value.Content;
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            return (memory.ToArray(), result.Value.Record.OriginalFileName);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doc = await _context.EmployeeDocuments.FindAsync(id);
            if (doc == null) return false;

            var marker = "api/files/";
            var start = doc.FilePath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start >= 0 && int.TryParse(doc.FilePath[(start + marker.Length)..].Split('/')[0], out var fileId))
                await _fileService.DeleteAsync(fileId);

            _context.EmployeeDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
