
using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/incrementhistory")]
    public class IncrementHistoryController : ControllerBase
    {
        private readonly IIncrementHistoryService _service;

        public IncrementHistoryController(IIncrementHistoryService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Add(IncrementHistoryDto dto)
        {
            await _service.AddAsync(dto);
            return Ok();
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(int employeeId)
        {
            var data = await _service.GetByEmployeeAsync(employeeId);
            return Ok(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }
    }

}
