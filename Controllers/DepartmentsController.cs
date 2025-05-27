using HRSystem.API.Dtos;
using HRSystem.API.Services;
using HRSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentsService _service;

        public DepartmentsController(IDepartmentsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _service.GetAllAsync();
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await _service.GetByIdAsync(id);
            return dept == null ? NotFound() : Ok(dept);
        }
        [HttpGet("by-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetByCompany(int companyId)
        {
            var departments = await _service.GetByCompanyAsync(companyId);
            return Ok(departments);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateDepartmentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
