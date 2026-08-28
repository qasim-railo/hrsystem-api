using System.Globalization;
using System.Text;
using System.Text.Json;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HRSystem.API.Controllers;
[ApiController, Authorize(Policy = "Employees.Create")]
[Route("api/imports")]
public sealed class ImportsController : ControllerBase
{
    private readonly AppDbContext _db; private readonly ICurrentTenant _tenant;
    public ImportsController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }
    [HttpGet] public async Task<ActionResult<IReadOnlyList<ImportJobDto>>> List(CancellationToken ct) =>
        Ok((await _db.ImportJobs.AsNoTracking().OrderByDescending(x => x.CreatedAt).Take(50).ToListAsync(ct)).Select(Map));
    [HttpPost("preview")]
    public async Task<ActionResult<ImportJobDto>> Preview([FromForm] string entityType, [FromForm] IFormFile file, CancellationToken ct)
    {
        if (_tenant.TenantId is not int tenantId || !int.TryParse(User.FindFirst("user_id")?.Value, out var userId)) return Forbid();
        if (entityType is not ("Employee" or "Department")) return BadRequest("Only Employee and Department imports are supported.");
        if (file is null || file.Length == 0) return BadRequest("A non-empty CSV or Excel file is required.");
        var rows = await ReadRows(file, ct); var errors = Validate(entityType, rows);
        var job = new ImportJob { TenantId = tenantId, UserId = userId, EntityType = entityType, FileName = file.FileName, TotalRows = rows.Count, ValidRows = rows.Count - errors.Count, ErrorRows = errors.Count, Status = errors.Count == 0 ? "Ready" : "Validation errors", RowsJson = JsonSerializer.Serialize(rows), ErrorsJson = JsonSerializer.Serialize(errors) };
        _db.ImportJobs.Add(job); await _db.SaveChangesAsync(ct); return Ok(Map(job));
    }
    [HttpPost("{id:int}/execute")]
    public async Task<ActionResult<ImportJobDto>> Execute(int id, CancellationToken ct)
    {
        var job = await _db.ImportJobs.SingleOrDefaultAsync(x => x.ImportJobId == id, ct); if (job is null) return NotFound();
        var rows = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(job.RowsJson) ?? []; var errors = Validate(job.EntityType, rows); var imported = 0;
        for (var i = 0; i < rows.Count; i++) { if (errors.Any(e => e.StartsWith($"Row {i + 2}:", StringComparison.Ordinal))) continue; var row = rows[i];
            if (job.EntityType == "Department") { var companyId = int.Parse(Get(row, "companyId"), CultureInfo.InvariantCulture); if (!await _db.Companies.AnyAsync(x => x.CompanyId == companyId, ct)) { errors.Add($"Row {i + 2}: companyId is invalid."); continue; } _db.Department.Add(new Department { Name = Get(row, "name"), Description = Get(row, "description"), CompanyId = companyId }); }
            else { var companyId = int.Parse(Get(row, "companyId"), CultureInfo.InvariantCulture); var departmentId = int.Parse(Get(row, "departmentId"), CultureInfo.InvariantCulture); if (!await _db.Companies.AnyAsync(x => x.CompanyId == companyId, ct) || !await _db.Department.AnyAsync(x => x.DepartmentId == departmentId && x.CompanyId == companyId, ct)) { errors.Add($"Row {i + 2}: company or department is invalid."); continue; } if (await _db.Employees.AnyAsync(x => x.EmployeeCode == Get(row, "employeeCode") || x.Email == Get(row, "email"), ct)) { errors.Add($"Row {i + 2}: employee code or email already exists."); continue; } _db.Employees.Add(new Employee { EmployeeCode = Get(row, "employeeCode"), FirstName = Get(row, "firstName"), LastName = Get(row, "lastName"), Email = Get(row, "email"), CompanyId = companyId, DepartmentId = departmentId, DateOfBirth = DateTime.TryParse(Get(row, "dateOfBirth"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : DateTime.MinValue }); } imported++; }
        job.ErrorsJson = JsonSerializer.Serialize(errors); job.ErrorRows = errors.Count; job.ValidRows = rows.Count - errors.Count; job.ImportedRows = imported; job.Status = "Completed"; await _db.SaveChangesAsync(ct); return Ok(Map(job));
    }
    [HttpGet("{id:int}/errors")] public async Task<IActionResult> Errors(int id, CancellationToken ct) { var job = await _db.ImportJobs.AsNoTracking().SingleOrDefaultAsync(x => x.ImportJobId == id, ct); if (job is null) return NotFound(); return File(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, JsonSerializer.Deserialize<List<string>>(job.ErrorsJson) ?? [])), "text/plain", $"import-{id}-errors.txt"); }
    private static async Task<List<Dictionary<string, string>>> ReadRows(IFormFile file, CancellationToken ct) { using var stream = new MemoryStream(); await file.CopyToAsync(stream, ct); stream.Position = 0; if (Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase)) { using var reader = new StreamReader(stream); var lines = (await reader.ReadToEndAsync(ct)).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries); var headers = lines.FirstOrDefault()?.Split(',').Select(x => x.Trim()).ToArray() ?? []; return lines.Skip(1).Select(x => headers.Zip(x.Split(',')).ToDictionary(y => y.First, y => y.Second.Trim())).ToList(); } using var package = new ExcelPackage(stream); var sheet = package.Workbook.Worksheets[0]; if (sheet.Dimension is null) return []; var headersX = Enumerable.Range(1, sheet.Dimension.Columns).Select(c => sheet.Cells[1, c].Text.Trim()).ToArray(); return Enumerable.Range(2, Math.Max(0, sheet.Dimension.Rows - 1)).Select(r => headersX.Select((h, c) => (h, sheet.Cells[r, c + 1].Text)).ToDictionary(x => x.h, x => x.Item2)).ToList(); }
    private static List<string> Validate(string type, List<Dictionary<string, string>> rows) { var result = new List<string>(); var required = type == "Department" ? new[] { "name", "companyId" } : new[] { "employeeCode", "firstName", "lastName", "email", "companyId", "departmentId" }; for (var i = 0; i < rows.Count; i++) foreach (var key in required.Where(k => string.IsNullOrWhiteSpace(Get(rows[i], k)))) result.Add($"Row {i + 2}: missing {key}."); return result; }
    private static string Get(Dictionary<string, string> row, string key) => row.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value ?? string.Empty;
    private static ImportJobDto Map(ImportJob x) => new(x.ImportJobId, x.EntityType, x.FileName, x.Status, x.TotalRows, x.ValidRows, x.ImportedRows, x.ErrorRows, JsonSerializer.Deserialize<List<string>>(x.ErrorsJson) ?? []);
}
