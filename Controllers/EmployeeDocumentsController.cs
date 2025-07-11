﻿using CloudinaryDotNet.Actions;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDocumentsController : ControllerBase
    {
        private readonly IEmployeeDocumentsService _service;

        private readonly CloudinaryService _cloudinaryService;
         

        private readonly AppDbContext _context;
        //private readonly CloudinaryUploadResult uploadResult;

        public EmployeeDocumentsController(IEmployeeDocumentsService service, CloudinaryService cloudinaryService,
            AppDbContext dbContext)
        {
            _service = service;
            _cloudinaryService = cloudinaryService;
            _context = dbContext;
        }

        //[HttpPost("upload")]
        //public async Task<IActionResult> Upload([FromForm] EmployeeDocumentUploadDto dto)
        //{
        //    var filePath = await _service.UploadAsync(dto);
        //    return Ok(new { Message = "Uploaded successfully", FilePath = filePath });
        //}
        [HttpPost("upload")]
        public async Task<IActionResult> UploadEmployeePhoto([FromForm] EmployeeDocumentUploadDto dto)
        {
            if (dto == null)
                return BadRequest("DTO is null");

            if (dto.File == null)
                return BadRequest("File is null");

            if (dto.File.Length == 0)
                return BadRequest("File is empty");

            var uploadResult = await _cloudinaryService.UploadImageAsync(dto.File);
            //var (imageUrl, publicId) = await _cloudinaryService.UploadImageAsync(dto.File);
            var document = new EmployeeDocument
            {
                EmployeeId = dto.EmployeeId,
                FileName = dto.File.FileName,  // likely line 48
                UploadedAt = DateTime.UtcNow,
                FileType = dto.FileType,
                FilePath = uploadResult.Url.ToString(),
                CloudinaryPublicId = uploadResult.PublicId,
            };

            _context.EmployeeDocuments.Add(document);
            await _context.SaveChangesAsync();

            return Ok(new { document });
        }

        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var docs = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(docs);
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var (fileBytes, fileName) = await _service.DownloadAsync(id);
            if (fileBytes == null) return NotFound();

            return File(fileBytes, "application/octet-stream", fileName);
        }

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var deleted = await _service.DeleteAsync(id);
        //    if (!deleted) return NotFound();
        //    return Ok("Deleted successfully.");
        //}

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.EmployeeDocuments.FindAsync(id);
            if (document == null) return NotFound();

            var deletedFromCloud = await _cloudinaryService.DeleteImageAsync(document.CloudinaryPublicId);

            _context.EmployeeDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted from DB and Cloudinary", deletedFromCloud });
        }

    }

}
