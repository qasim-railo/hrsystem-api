using System.Security.Claims;
using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/files")]
[Authorize(Policy = "Files.View")]
public sealed class FilesController : ControllerBase
{
    private readonly IFileService _files;
    public FilesController(IFileService files) => _files = files;

    [HttpPost("upload")]
    [Authorize(Policy = "Files.Upload")]
    public async Task<ActionResult<FileRecordDto>> Upload([FromForm] FileUploadDto request, CancellationToken cancellationToken)
    {
        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            return Ok(await _files.UploadAsync(request, user, cancellationToken));
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpGet("{fileId:int}")]
    public async Task<ActionResult<FileRecordDto>> Get(int fileId, CancellationToken cancellationToken)
        => (await _files.GetAsync(fileId, cancellationToken)) is { } result ? Ok(result) : NotFound();

    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> Download(int fileId, CancellationToken cancellationToken)
    {
        var result = await _files.OpenReadAsync(fileId, cancellationToken);
        return result is null ? NotFound() : File(result.Value.Content, result.Value.Record.MimeType, result.Value.Record.OriginalFileName);
    }

    [HttpDelete("{fileId:int}")]
    [Authorize(Policy = "Files.Upload")]
    public async Task<IActionResult> Delete(int fileId, CancellationToken cancellationToken)
        => await _files.DeleteAsync(fileId, cancellationToken) ? NoContent() : NotFound();
}
