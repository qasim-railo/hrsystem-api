using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers;

[ApiController]
[Authorize(Policy = "Users.Manage")]
[Route("api/custom-fields")]
[Route("api/tenant/custom-fields")]
public class CustomFieldsController : ControllerBase
{
    private readonly CustomFieldService _service;
    public CustomFieldsController(CustomFieldService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeInactive = true) => Ok(await _service.GetDefinitionsAsync(includeInactive));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) => (await _service.GetDefinitionAsync(id)) is { } result ? Ok(result) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Create(CustomFieldDefinitionDto dto)
    {
        try { var result = await _service.CreateDefinitionAsync(dto); return CreatedAtAction(nameof(Get), new { id = result.CustomFieldDefinitionId }, result); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomFieldDefinitionDto dto)
    {
        try { var result = await _service.UpdateDefinitionAsync(id, dto); return result == null ? NotFound() : Ok(result); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) => await _service.DeleteDefinitionAsync(id) ? NoContent() : NotFound();
}
