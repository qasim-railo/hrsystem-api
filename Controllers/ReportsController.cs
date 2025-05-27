using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
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
