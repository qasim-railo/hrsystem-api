using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(AttendanceDto dto)
        {
            await _service.AddAsync(dto);
            return Ok();
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var records = await _service.GetByEmployeeAsync(employeeId);
            return Ok(records);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            await _service.ImportFromExcelAsync(file);
            return Ok();
        }
    }

}
