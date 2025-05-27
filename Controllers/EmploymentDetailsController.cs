using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/employeedetails")]
    [ApiController]
    public class EmploymentDetailsController : ControllerBase
    {

        private readonly IEmploymentDetailsService _employmentDetailsService;

        public EmploymentDetailsController(IEmploymentDetailsService employmentDetailsService)
        {
            _employmentDetailsService = employmentDetailsService;
        }

        //// GET: api/EmploymentDetails
        //[HttpGet]
        //public async Task<ActionResult<List<EmploymentDetailDto>>> GetAll()
        //{
        //    var details = await _employmentDetailsService.GetAllAsync();
        //    return Ok(details);
        //}

        // GET: api/EmploymentDetails/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmploymentDetailDto>> GetById(int id)
        {
            var detail = await _employmentDetailsService.GetByIdAsync(id);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        // GET: api/EmploymentDetails/employee/{employeeId}
        [HttpGet("employee/{employeeId:int}")]
        public async Task<ActionResult<EmploymentDetailDto>> GetByEmployeeId(int employeeId)
        {
            var detail = await _employmentDetailsService.GetByEmployeeIdAsync(employeeId);
            if (detail == null) return NotFound();
            return Ok(detail);
        }

        // POST: api/EmploymentDetails
        [HttpPost]
        public async Task<ActionResult<EmploymentDetailDto>> Create(CreateEmploymentDetailDto dto)
        {
            var createdDetail = await _employmentDetailsService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDetail.EmploymentDetailId }, createdDetail);
        }

        // PUT: api/EmploymentDetails/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult<EmploymentDetailDto>> Update(int id, UpdateEmploymentDetailDto dto)
        {
            var updatedDetail = await _employmentDetailsService.UpdateAsync(id, dto);
            if (updatedDetail == null) return NotFound();
            return Ok(updatedDetail);
        }

        // DELETE: api/EmploymentDetails/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _employmentDetailsService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
