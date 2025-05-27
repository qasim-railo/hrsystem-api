using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
   
        [ApiController]
        [Route("api/gratuityreport")]
        public class GratuityReportController : ControllerBase
        {
            private readonly IGratuityReportService _service;

            public GratuityReportController(IGratuityReportService service)
            {
                _service = service;
            }

            [HttpGet("{employeeId}")]
            public async Task<IActionResult> Get(int employeeId, [FromQuery] DateTime endDate)
            {
                var report = await _service.GenerateAsync(employeeId, endDate);
                return Ok(report);
            }
        }

    
}
