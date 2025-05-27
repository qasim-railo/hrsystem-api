using HRSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [Route("api/finalsettlement")]
    [ApiController]
    public class FinalSettlementController : ControllerBase
    {
        private readonly IFinalSettlementService _service;

        public FinalSettlementController(IFinalSettlementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _service.GetByIdAsync(id));
        }

        [HttpPost("generate/{employeeId}")]
        public async Task<IActionResult> Generate(int employeeId)
        {
            await _service.GenerateSettlementAsync(employeeId);
            return Ok();
        }

        [HttpPost("signoff/{id}")]
        public async Task<IActionResult> SignOff(int id, [FromQuery] bool isEmployeeSign = false, [FromQuery] bool isHRSign = false)
        {
            await _service.SignOffAsync(id, isEmployeeSign, isHRSign);
            return Ok();
        }
    }

}
