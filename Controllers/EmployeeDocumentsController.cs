using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Files.View")]
    public class EmployeeDocumentsController : ControllerBase
    {
        private readonly IEmployeeDocumentsService _service;

        public EmployeeDocumentsController(IEmployeeDocumentsService service)
        {
            _service = service;
        }

        //[HttpPost("upload")]
        //public async Task<IActionResult> Upload([FromForm] EmployeeDocumentUploadDto dto)
        //{
        //    var filePath = await _service.UploadAsync(dto);
        //    return Ok(new { Message = "Uploaded successfully", FilePath = filePath });
        //}
        [HttpPost("upload")]
        [Authorize(Policy = "Files.Upload")]
        public async Task<IActionResult> UploadEmployeePhoto([FromForm] EmployeeDocumentUploadDto dto)
        {
            if (dto == null)
                return BadRequest("DTO is null");

            if (dto.File == null)
                return BadRequest("File is null");

            if (dto.File.Length == 0)
                return BadRequest("File is empty");

            var path = await _service.UploadAsync(dto);
            return Ok(new { FilePath = path });
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
        [Authorize(Policy = "Files.Upload")]
        public async Task<IActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(id) ? Ok(new { message = "Deleted successfully" }) : NotFound();
        }

    }

}
