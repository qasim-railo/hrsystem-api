using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/payrolls")]
    public class PayrollsController : ControllerBase
    {
        private readonly IPayrollService _service;

        public PayrollsController(IPayrollService service)
        {
            _service = service;
        }

        [HttpGet("generate/{year}/{month}")]
        public async Task<IActionResult> Generate(int year, int month)
        {
            var result = await _service.GenerateMonthlyPayrollAsync(year, month);
            return Ok(result);
        }

        [HttpPost("approve/{payrollId}")]
        public async Task<IActionResult> Approve(int payrollId)
        {
            await _service.ApprovePayrollAsync(payrollId);
            return NoContent();
        }

        [HttpGet("payslip/{payrollId}")]
        public async Task<IActionResult> GetPayslip(int payrollId)
        {
            var pdf = await _service.GeneratePayslipPdfAsync(payrollId);
            return File(pdf, "application/pdf", "Payslip.pdf");
        }

        [HttpGet("export/{year}/{month}")]
        public async Task<IActionResult> Export(int year, int month)
        {
            var excel = await _service.ExportPayrollToExcelAsync(year, month);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Payroll.xlsx");
        }
    }

}
