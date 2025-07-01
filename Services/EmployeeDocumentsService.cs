using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services
{
    public class EmployeeDocumentsService : IEmployeeDocumentsService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EmployeeDocumentsService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<string> UploadAsync(EmployeeDocumentUploadDto dto)
        {
            if (string.IsNullOrEmpty(_env.WebRootPath))
            {
                throw new InvalidOperationException("WebRootPath is not set. Ensure wwwroot folder exists and environment is configured correctly.");
            }
            var uploadFolder = Path.Combine(_env.WebRootPath, "Uploads", "Employees", dto.EmployeeId.ToString());
            Directory.CreateDirectory(uploadFolder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(dto.File.FileName);
            var fullPath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var document = new EmployeeDocument
            {
                EmployeeId = dto.EmployeeId,
                FileName = dto.File.FileName,
                FilePath = Path.Combine("Uploads", "Employees", dto.EmployeeId.ToString(), uniqueFileName).Replace("\\", "/"),
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

            var fullPath = Path.Combine(_env.WebRootPath, doc.FilePath);
            if (!File.Exists(fullPath)) return (null, null);

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return (bytes, doc.FileName);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doc = await _context.EmployeeDocuments.FindAsync(id);
            if (doc == null) return false;

            var fullPath = Path.Combine(_env.WebRootPath, doc.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            _context.EmployeeDocuments.Remove(doc);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
