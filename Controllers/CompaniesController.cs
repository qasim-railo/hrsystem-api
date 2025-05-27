using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly ICompaniesService _service;

    public CompaniesController(ICompaniesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompanyDto>>> GetAll()
    {
        return await _service.GetAllAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDto>> Get(int id)
    {
        var company = await _service.GetByIdAsync(id);
        if (company == null) return NotFound();
        return company;
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(CompanyDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.CompanyId }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CompanyDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
