using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.API.Controllers
{
    [ApiController]
    [Route("api/employees")]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        private readonly IAuditService _audit;

        public EmployeesController(IEmployeeService service, IAuditService audit)
        {
            _service = service;
            _audit = audit;
        }

        [HttpGet]
        [Authorize(Policy = "Employees.View")]
        public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterDto filter)
        {
            // Enforce company access scope: if user is not Admin, restrict companies to claims
            if (!User.IsInRole("Admin"))
            {
                var allowed = GetAllowedCompanyIdsFromClaims();
                if (allowed != null && allowed.Any())
                {
                    if (filter == null) filter = new EmployeeFilterDto();
                    if (filter.CompanyIds == null || !filter.CompanyIds.Any())
                    {
                        filter.CompanyIds = allowed;
                    }
                    else
                    {
                        filter.CompanyIds = filter.CompanyIds.Intersect(allowed).ToList();
                    }
                }
                else
                {
                    // No allowed companies; return empty
                    return Ok(new { items = new object[0], total = 0 });
                }
            }

            var (items, total) = await _service.GetFilteredAsync(filter ?? new EmployeeFilterDto());
            return Ok(new { items, total });
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Employees.View")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var allowed = GetAllowedCompanyIdsFromClaims();
                if (allowed != null && allowed.Any() && !allowed.Contains(result.CompanyId))
                    return Forbid();
            }

            return Ok(result);
        }

        [HttpGet("{id}/profile")]
        [Authorize(Policy = "Employees.View")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var result = await _service.GetProfileAsync(id);
            if (result == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var allowed = GetAllowedCompanyIdsFromClaims();
                if (allowed != null && allowed.Any() && !allowed.Contains(result.Employee.CompanyId))
                    return Forbid();
            }

            return Ok(result);
        }

        [HttpGet("{id}/history")]
        [Authorize(Policy = "Employees.View")]
        public async Task<IActionResult> GetHistory(int id)
        {
            var profile = await _service.GetProfileAsync(id);
            if (profile == null) return NotFound();
            if (!User.IsInRole("Admin"))
            {
                var allowed = GetAllowedCompanyIdsFromClaims();
                if (allowed != null && allowed.Any() && !allowed.Contains(profile.Employee.CompanyId))
                    return Forbid();
            }

            return Ok(new
            {
                profile.Employee.EmployeeId,
                profile.Employee.TenantId,
                profile.StatusHistory,
                profile.EmploymentHistory,
                profile.SalaryHistory
            });
        }

        [HttpPost]
        [Authorize(Policy = "Employees.Create")]
        public async Task<IActionResult> Create(EmployeeDto dto)
        {
            // Run server-side duplicate check
            var dupDto = new DuplicateCheckDto
            {
                PassportNumber = dto.PassportNumber,
                // NationalId is not present on EmployeeDto; labor card/national id may be present in EmploymentDetail. Leave null here.
                NationalId = null,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var dup = await _service.DuplicateCheckAsync(dupDto);
            var overrideHeader = Request.Headers.ContainsKey("X-Override") && Request.Headers["X-Override"].ToString().ToLower() == "true";
            var overrideReason = Request.Headers.ContainsKey("X-Override-Reason") ? Request.Headers["X-Override-Reason"].ToString() : null;

            if (dup.HasPotentialDuplicates && !overrideHeader)
            {
                return Conflict(dup);
            }

            if (dup.HasPotentialDuplicates && overrideHeader)
            {
                // check permission
                if (!User.IsInRole("Admin") && !User.HasClaim("permission", "Employees.OverrideDuplicate"))
                {
                    return Forbid();
                }
                if (string.IsNullOrWhiteSpace(overrideReason))
                {
                    return BadRequest("Override reason is required when overriding duplicate warning.");
                }
            }

            EmployeeDto result;
            try
            {
                result = await _service.CreateAsync(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            if (dup.HasPotentialDuplicates && overrideHeader)
            {
                // Audit the override
                await _audit.LogAsync("DuplicateOverride", "Employee", result.EmployeeId.ToString(), User.Identity?.Name ?? "",
                    System.Text.Json.JsonSerializer.Serialize(new { duplicateResult = dup, overrideReason }));
            }

            return CreatedAtAction(nameof(Get), new { id = result.EmployeeId }, result);
        }

        [HttpPost("duplicate-check")]
        [Authorize(Policy = "Employees.View")]
        public async Task<IActionResult> DuplicateCheck(DuplicateCheckDto dto)
        {
            var result = await _service.DuplicateCheckAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Employees.Edit")]
        public async Task<IActionResult> Update(int id, EmployeeDto dto)
        {
            EmployeeDto result;
            try
            {
                result = await _service.UpdateAsync(id, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "Employees.ChangeStatus")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] HRSystem.API.DTOs.ChangeStatusDto dto)
        {
            try
            {
                var ok = await _service.ChangeStatusAsync(id, (HRSystem.API.Models.EmployeeStatus)dto.NewStatus, dto.EffectiveDate, dto.LastWorkingDate, dto.Reason, dto.ChangedByUserId, dto.SupportingDocumentId);
                if (!ok) return NotFound();

                // Audit
                await _audit.LogAsync("ChangeStatus", "Employee", id.ToString(), User.Identity?.Name ?? "", System.Text.Json.JsonSerializer.Serialize(dto));

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Employees.Edit")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        private List<int> GetAllowedCompanyIdsFromClaims()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "CompanyIds" || c.Type == "companyids" || c.Type == "companyIds");
            if (claim == null) return null;
            var parts = claim.Value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Select(p => int.TryParse(p, out var v) ? v : -1).Where(v => v > 0).ToList();
        }
    }
}
