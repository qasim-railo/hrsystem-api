using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController, Authorize(Policy = "Users.Manage")]
[Route("api/attendance-configuration")]
public class AttendanceConfigurationController : ControllerBase
{
    private readonly AppDbContext _db;
    public AttendanceConfigurationController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<AttendanceConfigurationDto>> Get()
    {
        var config = await _db.AttendanceConfigurations.SingleOrDefaultAsync();
        if (config == null)
        {
            config = new AttendanceConfiguration();
            _db.AttendanceConfigurations.Add(config);
            await _db.SaveChangesAsync();
        }
        return Ok(Map(config));
    }

    [HttpPut]
    public async Task<ActionResult<AttendanceConfigurationDto>> Save(AttendanceConfigurationDto dto)
    {
        if (dto.GraceInMinutes < 0 || dto.GraceOutMinutes < 0 || dto.DefaultWorkingHours <= 0)
            return BadRequest("Grace periods must be non-negative and working hours must be greater than zero.");
        if (!new[] { "Flag", "Ignore", "AutoAbsent" }.Contains(dto.MissingPunchPolicy) ||
            !new[] { "Track", "Ignore", "Flag" }.Contains(dto.LateEarlyRule))
            return BadRequest("Invalid attendance rule.");
        var config = await _db.AttendanceConfigurations.SingleOrDefaultAsync();
        if (config == null) _db.AttendanceConfigurations.Add(config = new AttendanceConfiguration());
        config.AllowedSources = string.Join(",", dto.AllowedSources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase));
        config.GraceInMinutes = dto.GraceInMinutes; config.GraceOutMinutes = dto.GraceOutMinutes;
        config.MissingPunchPolicy = dto.MissingPunchPolicy; config.LateEarlyRule = dto.LateEarlyRule;
        config.ApprovalRequired = dto.ApprovalRequired; config.DefaultWorkingHours = dto.DefaultWorkingHours;
        await _db.SaveChangesAsync();
        return Ok(Map(config));
    }

    [HttpGet("imports")]
    public async Task<ActionResult<IEnumerable<AttendanceImportLogDto>>> Imports() => Ok(await _db.AttendanceImportLogs.AsNoTracking().OrderByDescending(x => x.ImportedAt).Select(x => new AttendanceImportLogDto { Id = x.Id, ImportedAt = x.ImportedAt, Source = x.Source, FileName = x.FileName, TotalRows = x.TotalRows, ImportedRows = x.ImportedRows, ErrorRows = x.ErrorRows, Errors = x.Errors }).Take(50).ToListAsync());

    private static AttendanceConfigurationDto Map(AttendanceConfiguration x) => new() { AllowedSources = x.AllowedSources, GraceInMinutes = x.GraceInMinutes, GraceOutMinutes = x.GraceOutMinutes, MissingPunchPolicy = x.MissingPunchPolicy, LateEarlyRule = x.LateEarlyRule, ApprovalRequired = x.ApprovalRequired, DefaultWorkingHours = x.DefaultWorkingHours };
}
