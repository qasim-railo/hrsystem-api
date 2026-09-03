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
        var workingDays = await EnsureWorkingDaysAsync();
        return Ok(Map(config, workingDays));
    }

    [HttpPut]
    public async Task<ActionResult<AttendanceConfigurationDto>> Save(AttendanceConfigurationDto dto)
    {
        if (dto.GraceInMinutes < 0 || dto.GraceOutMinutes < 0 || dto.ExpectedWorkMinutes <= 0)
            return BadRequest("Grace periods must be non-negative and expected work minutes must be greater than zero.");
        if (!new[] { "Flag", "Ignore", "AutoAbsent" }.Contains(dto.MissingPunchPolicy) ||
            !new[] { "Track", "Ignore", "Flag" }.Contains(dto.LateEarlyRule))
            return BadRequest("Invalid attendance rule.");
        if (dto.WorkingDays.Count != 7 || dto.WorkingDays.Select(day => day.DayOfWeek).Distinct().Count() != 7 ||
            dto.WorkingDays.Any(day => day.BreakMinutes < 0 || day.DefaultStartTime == day.DefaultEndTime))
            return BadRequest("Configure each day with valid start, end, and break times.");
        var config = await _db.AttendanceConfigurations.SingleOrDefaultAsync();
        if (config == null) _db.AttendanceConfigurations.Add(config = new AttendanceConfiguration());
        config.AllowedSources = string.Join(",", dto.AllowedSources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct(StringComparer.OrdinalIgnoreCase));
        config.GraceInMinutes = dto.GraceInMinutes; config.GraceOutMinutes = dto.GraceOutMinutes;
        config.MissingPunchPolicy = dto.MissingPunchPolicy; config.LateEarlyRule = dto.LateEarlyRule;
        config.ApprovalRequired = dto.ApprovalRequired;
        config.ExpectedWorkMinutes = dto.ExpectedWorkMinutes;
        config.DefaultWorkingHours = Math.Round(dto.ExpectedWorkMinutes / 60m, 2);
        var existingWorkingDays = await _db.TenantWorkingDays.ToListAsync();
        foreach (var day in dto.WorkingDays)
        {
            var entity = existingWorkingDays.SingleOrDefault(item => item.DayOfWeek == day.DayOfWeek);
            if (entity == null)
                _db.TenantWorkingDays.Add(new TenantWorkingDay
                {
                    DayOfWeek = day.DayOfWeek, IsWorkingDay = day.IsWorkingDay, DefaultStartTime = day.DefaultStartTime,
                    DefaultEndTime = day.DefaultEndTime, BreakMinutes = day.BreakMinutes
                });
            else
            {
                entity.IsWorkingDay = day.IsWorkingDay;
                entity.DefaultStartTime = day.DefaultStartTime;
                entity.DefaultEndTime = day.DefaultEndTime;
                entity.BreakMinutes = day.BreakMinutes;
            }
        }
        await _db.SaveChangesAsync();
        return Ok(Map(config, await _db.TenantWorkingDays.AsNoTracking().OrderBy(day => day.DayOfWeek).ToListAsync()));
    }

    [HttpGet("imports")]
    public async Task<ActionResult<IEnumerable<AttendanceImportLogDto>>> Imports() => Ok(await _db.AttendanceImportLogs.AsNoTracking().OrderByDescending(x => x.ImportedAt).Select(x => new AttendanceImportLogDto { Id = x.Id, ImportedAt = x.ImportedAt, Source = x.Source, FileName = x.FileName, TotalRows = x.TotalRows, ImportedRows = x.ImportedRows, ErrorRows = x.ErrorRows, Errors = x.Errors }).Take(50).ToListAsync());

    private async Task<List<TenantWorkingDay>> EnsureWorkingDaysAsync()
    {
        var workingDays = await _db.TenantWorkingDays.OrderBy(day => day.DayOfWeek).ToListAsync();
        if (workingDays.Count == 7) return workingDays;

        var existing = workingDays.Select(day => day.DayOfWeek).ToHashSet();
        foreach (var dayOfWeek in Enum.GetValues<DayOfWeek>().Where(dayOfWeek => !existing.Contains(dayOfWeek)))
        {
            _db.TenantWorkingDays.Add(new TenantWorkingDay
            {
                DayOfWeek = dayOfWeek,
                IsWorkingDay = true,
                DefaultStartTime = new TimeSpan(8, 0, 0),
                DefaultEndTime = new TimeSpan(17, 0, 0),
                BreakMinutes = 60
            });
        }
        await _db.SaveChangesAsync();
        return await _db.TenantWorkingDays.OrderBy(day => day.DayOfWeek).ToListAsync();
    }

    private static AttendanceConfigurationDto Map(AttendanceConfiguration config, IEnumerable<TenantWorkingDay> workingDays) => new()
    {
        AllowedSources = config.AllowedSources, GraceInMinutes = config.GraceInMinutes, GraceOutMinutes = config.GraceOutMinutes,
        MissingPunchPolicy = config.MissingPunchPolicy, LateEarlyRule = config.LateEarlyRule, ApprovalRequired = config.ApprovalRequired,
        DefaultWorkingHours = config.DefaultWorkingHours,
        ExpectedWorkMinutes = config.ExpectedWorkMinutes > 0
            ? config.ExpectedWorkMinutes
            : (int)Math.Round(config.DefaultWorkingHours * 60m),
        WorkingDays = workingDays.Select(day => new TenantWorkingDayDto
        {
            DayOfWeek = day.DayOfWeek, IsWorkingDay = day.IsWorkingDay, DefaultStartTime = day.DefaultStartTime,
            DefaultEndTime = day.DefaultEndTime, BreakMinutes = day.BreakMinutes
        }).ToList()
    };
}
