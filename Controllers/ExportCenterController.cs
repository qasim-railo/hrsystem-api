using System.Globalization;
using System.Text;
using System.Text.Json;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/exports")]
[Authorize]
public sealed class ExportCenterController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IReportService _reportService;
    private readonly IAuditService _audit;

    public ExportCenterController(AppDbContext db, IReportService reportService, IAuditService audit)
    {
        _db = db;
        _reportService = reportService;
        _audit = audit;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ExportOptionDto>> GetAvailable()
    {
        var permissions = User.Claims.Where(x => x.Type == "permission").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasAdmin = permissions.Contains("Users.Manage");
        var exports = new[]
        {
            new ExportOptionDto("employees", "Employees", "Employee master list with core personnel details.", "Employees.Export", true, true, "/api/exports/employees"),
            new ExportOptionDto("attendance", "Attendance", "Daily attendance summary for the current tenant.", "Employees.Export", false, true, "/api/exports/attendance"),
            new ExportOptionDto("leave", "Leave", "Leave usage and approval records.", "Employees.Export", false, true, "/api/exports/leave"),
            new ExportOptionDto("payroll", "Payroll", "Payroll data and net pay totals.", "Employees.Export", true, true, "/api/exports/payroll"),
            new ExportOptionDto("loans", "Loans", "Loan balances and repayment status.", "Employees.Export", true, false, "/api/exports/loans"),
            new ExportOptionDto("assets", "Assets", "Assigned equipment and asset status.", "Employees.Export", false, true, "/api/exports/assets"),
            new ExportOptionDto("documents", "Document Index", "Stored document inventory for your tenant.", "Files.View", false, true, "/api/exports/documents")
        };

        return Ok(exports.Where(x => hasAdmin || permissions.Contains(x.Permission)).ToList());
    }

    [HttpGet("{type}")]
    public async Task<IActionResult> Download(string type, [FromQuery] ReportFilterDto? filter)
    {
        var option = GetDefinition(type);
        if (option == null) return NotFound();
        if (!HasPermission(option.Permission)) return Forbid();

        var fileName = $"{option.Code}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

        switch (type.Trim().ToLowerInvariant())
        {
            case "employees":
                await AuditSensitiveExportAsync(option.Code);
                var employeeBytes = await _reportService.ExportEmployeeReportAsync(filter ?? new ReportFilterDto());
                return File(employeeBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "attendance":
                return File(await BuildWorkbookFromAttendanceAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "leave":
                return File(await BuildWorkbookFromLeaveAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "payroll":
                await AuditSensitiveExportAsync(option.Code);
                return File(await BuildWorkbookFromPayrollAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "loans":
                await AuditSensitiveExportAsync(option.Code);
                return File(await BuildWorkbookFromLoansAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "assets":
                return File(await BuildWorkbookFromAssetsAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            case "documents":
                return File(await BuildWorkbookFromDocumentsAsync(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            default:
                return NotFound();
        }
    }

    private static ExportOptionDto? GetDefinition(string type) => type.Trim().ToLowerInvariant() switch
    {
        "employees" => new ExportOptionDto("employees", "Employees", "Employee master list with core personnel details.", "Employees.Export", true, true, "/api/exports/employees"),
        "attendance" => new ExportOptionDto("attendance", "Attendance", "Daily attendance summary for the current tenant.", "Employees.Export", false, true, "/api/exports/attendance"),
        "leave" => new ExportOptionDto("leave", "Leave", "Leave usage and approval records.", "Employees.Export", false, true, "/api/exports/leave"),
        "payroll" => new ExportOptionDto("payroll", "Payroll", "Payroll data and net pay totals.", "Employees.Export", true, true, "/api/exports/payroll"),
        "loans" => new ExportOptionDto("loans", "Loans", "Loan balances and repayment status.", "Employees.Export", true, false, "/api/exports/loans"),
        "assets" => new ExportOptionDto("assets", "Assets", "Assigned equipment and asset status.", "Employees.Export", false, true, "/api/exports/assets"),
        "documents" => new ExportOptionDto("documents", "Document Index", "Stored document inventory for your tenant.", "Files.View", false, true, "/api/exports/documents"),
        _ => null
    };

    private bool HasPermission(string permission)
    {
        var permissions = User.Claims.Where(x => x.Type == "permission").Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return permissions.Contains(permission) || permissions.Contains("Users.Manage");
    }

    private async Task AuditSensitiveExportAsync(string type)
    {
        var userId = User.FindFirst("user_id")?.Value ?? User.Identity?.Name ?? "unknown";
        await _audit.LogAsync("Export", type, "export-center", userId, JsonSerializer.Serialize(new { type, exportedAt = DateTime.UtcNow, sensitive = true }));
    }

    private async Task<byte[]> BuildWorkbookFromAttendanceAsync()
    {
        var rows = await _db.Attendance
            .Include(x => x.Employee)
            .OrderBy(x => x.Date)
            .Select(x => new { x.EmployeeId, EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName, x.Date, x.CheckIn, x.CheckOut, x.OT1, x.OT2 })
            .ToListAsync();

        return BuildWorkbook(rows, new[] { "EmployeeId", "EmployeeName", "Date", "CheckIn", "CheckOut", "OT1Minutes", "OT2Minutes" }, "Attendance");
    }

    private async Task<byte[]> BuildWorkbookFromLeaveAsync()
    {
        var rows = await _db.LeaveRequests
            .Include(x => x.Employee)
            .OrderByDescending(x => x.StartDate)
            .Select(x => new { x.Id, EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName, x.StartDate, x.EndDate, x.Status, x.LeaveType, x.Reason })
            .ToListAsync();

        return BuildWorkbook(rows, new[] { "Id", "EmployeeName", "StartDate", "EndDate", "Status", "LeaveType", "Reason" }, "Leave");
    }

    private async Task<byte[]> BuildWorkbookFromPayrollAsync()
    {
        var rows = await _db.Payrolls
            .Include(x => x.Employee)
            .OrderByDescending(x => x.Month)
            .Select(x => new { x.Id, EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName, x.Month, BasicSalary = x.BasicSalary, OTEarnings = x.OTEarnings, Deductions = x.Deductions, NetSalary = x.NetSalary, x.IsApproved })
            .ToListAsync();

        return BuildWorkbook(rows, new[] { "Id", "EmployeeName", "Month", "BasicSalary", "OTEarnings", "Deductions", "NetSalary", "IsApproved" }, "Payroll");
    }

    private async Task<byte[]> BuildWorkbookFromLoansAsync()
    {
        var rows = new List<Dictionary<string, object?>>
        {
            new() { ["Status"] = "Loan export is not available in the current data model." }
        };

        return BuildWorkbook(rows, new[] { "Status" }, "Loans");
    }

    private async Task<byte[]> BuildWorkbookFromAssetsAsync()
    {
        var rows = await _db.EmployeeAssets
            .Include(x => x.Employee)
            .Include(x => x.Asset)
            .OrderBy(x => x.AssignedDate)
            .Select(x => new { x.Id, EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName, AssetName = x.Asset.Name, x.AssignedDate, x.ReturnedDate, x.Status })
            .ToListAsync();

        return BuildWorkbook(rows, new[] { "Id", "EmployeeName", "AssetName", "AssignedDate", "ReturnedDate", "Status" }, "Assets");
    }

    private async Task<byte[]> BuildWorkbookFromDocumentsAsync()
    {
        var rows = await _db.FileRecords
            .OrderByDescending(x => x.UploadedAt)
            .Select(x => new { x.FileId, x.EntityType, x.EntityId, x.DocumentType, x.OriginalFileName, x.MimeType, x.Size, x.UploadedAt, x.Status })
            .ToListAsync();

        return BuildWorkbook(rows, new[] { "FileId", "EntityType", "EntityId", "DocumentType", "OriginalFileName", "MimeType", "Size", "UploadedAt", "Status" }, "DocumentIndex");
    }

    private static byte[] BuildWorkbook<T>(IEnumerable<T> rows, string[] headers, string sheetName)
    {
        using var package = new ExcelPackage();
        var sheet = package.Workbook.Worksheets.Add(sheetName);
        var propertyValues = typeof(T).GetProperties().Select(p => p.Name).ToArray();

        for (var i = 0; i < headers.Length; i++)
        {
            sheet.Cells[1, i + 1].Value = headers[i];
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            if (row is Dictionary<string, object?> dict)
            {
                for (var i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[rowIndex, i + 1].Value = dict.TryGetValue(headers[i], out var value) ? value ?? string.Empty : string.Empty;
                }
            }
            else
            {
                for (var i = 0; i < propertyValues.Length; i++)
                {
                    var value = typeof(T).GetProperty(propertyValues[i])?.GetValue(row);
                    sheet.Cells[rowIndex, i + 1].Value = value ?? string.Empty;
                }
            }
            rowIndex++;
        }

        sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        return package.GetAsByteArray();
    }
}
