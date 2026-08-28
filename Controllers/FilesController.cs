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

    [HttpPost("{fileId:int}/replace")]
    [Authorize(Policy = "Files.Replace")]
    public async Task<ActionResult<FileRecordDto>> Replace(int fileId, [FromForm] FileReplaceDto request, CancellationToken cancellationToken)
    {
        try
        {
            var user = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
            var result = await _files.ReplaceAsync(fileId, request.File, user, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpGet("{fileId:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<FileRecordDto>>> VersionHistory(int fileId, CancellationToken cancellationToken)
        => Ok(await _files.GetVersionHistoryAsync(fileId, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FileRecordDto>>> Search([FromQuery] FileSearchRequest request, CancellationToken cancellationToken)
        => Ok(await _files.SearchAsync(request, cancellationToken));

    [HttpGet("{fileId:int}/download")]
    public async Task<IActionResult> Download(int fileId, CancellationToken cancellationToken)
    {
        var result = await _files.OpenReadAsync(fileId, cancellationToken);
        return result is null ? NotFound() : File(result.Value.Content, result.Value.Record.MimeType, result.Value.Record.OriginalFileName);
    }

    [HttpDelete("{fileId:int}")]
    [Authorize(Policy = "Files.Delete")]
    public async Task<IActionResult> Delete(int fileId, CancellationToken cancellationToken)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        return await _files.DeleteAsync(fileId, user, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("recycle-bin")]
    [Authorize(Policy = "Files.Restore")]
    public async Task<ActionResult<IReadOnlyList<FileRecordDto>>> RecycleBin(CancellationToken cancellationToken)
        => Ok(await _files.GetRecycleBinAsync(cancellationToken));

    [HttpPost("{fileId:int}/restore")]
    [Authorize(Policy = "Files.Restore")]
    public async Task<ActionResult<FileRecordDto>> Restore(int fileId, CancellationToken cancellationToken)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var result = await _files.RestoreAsync(fileId, user, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{fileId:int}/purge")]
    [Authorize(Policy = "Files.Purge")]
    public async Task<IActionResult> Purge(int fileId, CancellationToken cancellationToken)
    {
        var user = User.FindFirstValue(ClaimTypes.Name) ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        return await _files.PurgeAsync(fileId, user, cancellationToken) ? NoContent() : NotFound();
    }
}
