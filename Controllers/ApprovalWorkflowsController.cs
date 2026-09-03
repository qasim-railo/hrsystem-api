using System.Security.Claims;
using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/approval-workflows")]
[Authorize]
public class ApprovalWorkflowsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmployeeService _employees;
    public ApprovalWorkflowsController(AppDbContext db, IEmployeeService employees) { _db = db; _employees = employees; }

    [HttpGet]
    [Authorize(Policy = "Workflows.Manage")]
    public async Task<ActionResult<IEnumerable<ApprovalWorkflowDto>>> List() =>
        Ok(await _db.ApprovalWorkflows.Include(x => x.Steps).OrderBy(x => x.Module).ThenBy(x => x.Name).Select(MapExpression).ToListAsync());

    [HttpGet("{id:int}")]
    [Authorize(Policy = "Workflows.Manage")]
    public async Task<ActionResult<ApprovalWorkflowDto>> Get(int id)
    {
        var workflow = await _db.ApprovalWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.Id == id);
        return workflow == null ? NotFound() : Ok(Map(workflow));
    }

    [HttpPost]
    [Authorize(Policy = "Workflows.Manage")]
    public async Task<ActionResult<ApprovalWorkflowDto>> Create(SaveApprovalWorkflowDto dto)
    {
        var validation = Validate(dto);
        if (validation != null) return BadRequest(validation);
        var workflow = new ApprovalWorkflow();
        Apply(workflow, dto);
        _db.ApprovalWorkflows.Add(workflow);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = workflow.Id }, Map(workflow));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "Workflows.Manage")]
    public async Task<ActionResult<ApprovalWorkflowDto>> Update(int id, SaveApprovalWorkflowDto dto)
    {
        var validation = Validate(dto);
        if (validation != null) return BadRequest(validation);
        var workflow = await _db.ApprovalWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.Id == id);
        if (workflow == null) return NotFound();
        Apply(workflow, dto);
        await _db.SaveChangesAsync();
        return Ok(Map(workflow));
    }

    [HttpPost("{id:int}/requests")]
    [Authorize(Policy = "Workflows.Manage")]
    public async Task<ActionResult> CreateRequest(int id, CreateApprovalRequestDto dto)
    {
        var workflow = await _db.ApprovalWorkflows.Include(x => x.Steps).SingleOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (workflow == null || workflow.Steps.Count == 0) return NotFound("Active workflow with steps was not found.");
        var request = new ApprovalRequest
        {
            ApprovalWorkflowId = id, Module = workflow.Module, RequestType = workflow.RequestType,
            Reference = dto.Reference?.Trim() ?? string.Empty, RequestedByUserId = CurrentUserId(),
            CurrentStepOrder = workflow.Steps.Min(x => x.StepOrder)
        };
        _db.ApprovalRequests.Add(request);
        await _db.SaveChangesAsync();
        return Ok(new { request.Id, request.Status, request.CurrentStepOrder });
    }

    [HttpPost("requests/{id:int}/actions")]
    public async Task<ActionResult> Act(int id, ApprovalActionDto dto)
    {
        var request = await _db.ApprovalRequests.Include(x => x.Workflow).ThenInclude(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (request == null) return NotFound();
        if (request.Status != "Pending") return Conflict("This approval request is already complete.");
        var decision = dto.Decision.Trim().ToLowerInvariant();
        if (decision is not ("approved" or "rejected")) return BadRequest("Decision must be Approved or Rejected.");
        var step = request.Workflow.Steps.SingleOrDefault(x => x.StepOrder == request.CurrentStepOrder);
        if (step == null) return Conflict("The current workflow step is invalid.");
        if (!User.IsInRole(step.ApproverRole)) return Forbid();
        _db.ApprovalActions.Add(new ApprovalAction { ApprovalRequestId = id, StepOrder = step.StepOrder, ActionByUserId = CurrentUserId(), Decision = decision, Comments = dto.Comments?.Trim() ?? string.Empty });
        if (decision == "rejected") { request.Status = "Rejected"; request.CompletedAt = DateTime.UtcNow; }
        else
        {
            var next = request.Workflow.Steps.Where(x => x.StepOrder > request.CurrentStepOrder).OrderBy(x => x.StepOrder).FirstOrDefault();
            if (next == null) { request.Status = "Approved"; request.CompletedAt = DateTime.UtcNow; }
            else request.CurrentStepOrder = next.StepOrder;
        }
        await _db.SaveChangesAsync();
        if (request.Module.Equals("Employee", StringComparison.OrdinalIgnoreCase) && request.CompletedAt.HasValue &&
            request.Reference.StartsWith("employee:", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(request.Reference["employee:".Length..], out var employeeId))
            await _employees.SetRecordStatusAsync(employeeId, request.Status == "Approved" ? EmployeeRecordStatus.Approved : EmployeeRecordStatus.Rejected);
        return Ok(new { request.Id, request.Status, request.CurrentStepOrder });
    }

    private int CurrentUserId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var id) ? id : 0;
    private static string? Validate(SaveApprovalWorkflowDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Module) || string.IsNullOrWhiteSpace(dto.RequestType)) return "Name, module, and request type are required.";
        if (dto.Steps.Count == 0) return "At least one approval step is required.";
        if (dto.Steps.Any(x => x.StepOrder < 1 || string.IsNullOrWhiteSpace(x.Name) || string.IsNullOrWhiteSpace(x.ApproverRole))) return "Each step needs an order, name, and approver role.";
        if (dto.Steps.Select(x => x.StepOrder).Distinct().Count() != dto.Steps.Count) return "Step orders must be unique.";
        if (dto.Steps.Any(x => !new[] { "Sequential", "Parallel" }.Contains(x.ApprovalMode, StringComparer.OrdinalIgnoreCase))) return "Approval mode must be Sequential or Parallel.";
        return null;
    }
    private static void Apply(ApprovalWorkflow workflow, SaveApprovalWorkflowDto dto)
    {
        workflow.Name = dto.Name.Trim(); workflow.Module = dto.Module.Trim(); workflow.RequestType = dto.RequestType.Trim(); workflow.IsActive = dto.IsActive;
        workflow.Steps.Clear();
        foreach (var step in dto.Steps.OrderBy(x => x.StepOrder))
            workflow.Steps.Add(new ApprovalStep { StepOrder = step.StepOrder, Name = step.Name.Trim(), ApproverRole = step.ApproverRole.Trim(), ApprovalMode = step.ApprovalMode.Trim(), EscalationAfterHours = step.EscalationAfterHours });
    }
    private static ApprovalWorkflowDto Map(ApprovalWorkflow x) => new() { Id = x.Id, Name = x.Name, Module = x.Module, RequestType = x.RequestType, IsActive = x.IsActive, Steps = x.Steps.OrderBy(s => s.StepOrder).Select(s => new ApprovalStepDto { Id = s.Id, StepOrder = s.StepOrder, Name = s.Name, ApproverRole = s.ApproverRole, ApprovalMode = s.ApprovalMode, EscalationAfterHours = s.EscalationAfterHours }).ToList() };
    private static readonly System.Linq.Expressions.Expression<Func<ApprovalWorkflow, ApprovalWorkflowDto>> MapExpression = x => new ApprovalWorkflowDto { Id = x.Id, Name = x.Name, Module = x.Module, RequestType = x.RequestType, IsActive = x.IsActive, Steps = x.Steps.OrderBy(s => s.StepOrder).Select(s => new ApprovalStepDto { Id = s.Id, StepOrder = s.StepOrder, Name = s.Name, ApproverRole = s.ApproverRole, ApprovalMode = s.ApprovalMode, EscalationAfterHours = s.EscalationAfterHours }).ToList() };
}
