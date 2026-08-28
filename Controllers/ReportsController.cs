using HRSystem.API.Services;
using HRSystem.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> EmployeeReport([FromQuery] ReportFilterDto filter)
            => Ok(await _reportService.GetEmployeeReportAsync(filter));

        [HttpGet("employees/export")]
        public async Task<IActionResult> ExportEmployeeReport([FromQuery] ReportFilterDto filter)
        {
            if (!User.Claims.Any(c => c.Type == "permission" && (c.Value == "Reports.Export" || c.Value == "Users.Manage")))
                return Forbid();
            var bytes = await _reportService.ExportEmployeeReportAsync(filter);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"employees-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("salary")]
        public async Task<IActionResult> SalaryReport(int? companyId, int? employeeId, int? month)
        {
            var data = await _reportService.GetSalaryReportAsync(companyId, employeeId, month);
            return Ok(data);
        }

        [HttpGet("leave")]
        public async Task<IActionResult> LeaveReport(int? companyId, int? employeeId)
        {
            var data = await _reportService.GetLeaveReportAsync(companyId, employeeId);
            return Ok(data);
        }

        [HttpGet("asset")]
        public async Task<IActionResult> AssetReport(int? companyId, int? employeeId)
        {
            var data = await _reportService.GetAssetReportAsync(companyId, employeeId);
            return Ok(data);
        }

        [HttpGet("loan")]
        public async Task<IActionResult> LoanReport(int? companyId, int? employeeId)
        {
            var data = await _reportService.GetLoanReportAsync(companyId, employeeId);
            return Ok(data);
        }
    }

}
