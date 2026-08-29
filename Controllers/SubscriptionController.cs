using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using HRSystem.API.Services;
using HRSystem.API.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Controllers;

[ApiController]
[Route("api/platform/subscriptions")]
[Authorize(Policy = "Platform.Tenants")]
public class SubscriptionController : ControllerBase
{
    private readonly AppDbContext _db;
    public SubscriptionController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SubscriptionDto>>> List()
        => Ok(await Query().OrderBy(s => s.TenantName).ToListAsync());

    [HttpPost("/api/platform/tenants/{tenantId:int}/subscription/activate")]
    public async Task<ActionResult<SubscriptionDto>> Activate(int tenantId, ActivateSubscriptionDto dto)
    {
        var subscription = await Find(tenantId);
        if (subscription == null) return NotFound();
        if (dto.PlanId is int planId)
        {
            var plan = await _db.Plans.FindAsync(planId);
            if (plan == null) return BadRequest("The selected plan does not exist.");
            subscription.PlanId = planId;
        }
        subscription.Status = SubscriptionStatus.Active;
        subscription.RenewalDate = dto.RenewalDate ?? subscription.RenewalDate ?? DateTime.UtcNow.AddMonths(1);
        subscription.BillingCycle = string.IsNullOrWhiteSpace(dto.BillingCycle) ? subscription.BillingCycle : dto.BillingCycle.Trim();
        subscription.Notes = dto.Notes ?? subscription.Notes;
        await Save(subscription, "Activated");
        return Ok(await Query().SingleAsync(s => s.SubscriptionId == subscription.SubscriptionId));
    }

    [HttpPut("/api/platform/tenants/{tenantId:int}/subscription/plan")]
    public async Task<ActionResult<SubscriptionDto>> ChangePlan(int tenantId, ChangeSubscriptionPlanDto dto)
    {
        var subscription = await Find(tenantId);
        var plan = await _db.Plans.FindAsync(dto.PlanId);
        if (subscription == null || plan == null) return NotFound();
        subscription.PlanId = dto.PlanId;
        subscription.Notes = dto.Notes ?? subscription.Notes;
        await Save(subscription, "PlanChanged");
        return Ok(await Query().SingleAsync(s => s.SubscriptionId == subscription.SubscriptionId));
    }

    [HttpPost("/api/platform/tenants/{tenantId:int}/subscription/extend")]
    public async Task<ActionResult<SubscriptionDto>> Extend(int tenantId, ExtendSubscriptionDto dto)
    {
        var subscription = await Find(tenantId);
        if (subscription == null) return NotFound();
        if (dto.RenewalDate <= DateTime.UtcNow) return BadRequest("RenewalDate must be in the future.");
        subscription.RenewalDate = dto.RenewalDate;
        subscription.Notes = dto.Notes ?? subscription.Notes;
        await Save(subscription, "Extended");
        return Ok(await Query().SingleAsync(s => s.SubscriptionId == subscription.SubscriptionId));
    }

    [HttpPost("/api/platform/tenants/{tenantId:int}/subscription/suspend")]
    public Task<ActionResult<SubscriptionDto>> Suspend(int tenantId, [FromBody] ChangeSubscriptionPlanDto? dto)
        => SetStatus(tenantId, SubscriptionStatus.Suspended, "Suspended", dto?.Notes);

    [HttpPost("/api/platform/tenants/{tenantId:int}/subscription/cancel")]
    public Task<ActionResult<SubscriptionDto>> Cancel(int tenantId, [FromBody] ChangeSubscriptionPlanDto? dto)
        => SetStatus(tenantId, SubscriptionStatus.Cancelled, "Cancelled", dto?.Notes);

    private async Task<ActionResult<SubscriptionDto>> SetStatus(int tenantId, SubscriptionStatus status, string action, string? notes)
    {
        var subscription = await Find(tenantId);
        if (subscription == null) return NotFound();
        subscription.Status = status;
        subscription.CancelledAt = status == SubscriptionStatus.Cancelled ? DateTime.UtcNow : subscription.CancelledAt;
        subscription.Notes = notes ?? subscription.Notes;
        await Save(subscription, action);
        return Ok(await Query().SingleAsync(s => s.SubscriptionId == subscription.SubscriptionId));
    }

    private async Task<Subscription?> Find(int tenantId)
        => await _db.Subscriptions.Include(s => s.Tenant).SingleOrDefaultAsync(s => s.TenantId == tenantId);

    private async Task Save(Subscription subscription, string action)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        subscription.Tenant.Status = subscription.Status.ToString();
        subscription.Tenant.LifecycleStatus = subscription.Status.ToString();
        subscription.Tenant.BillingStatus = subscription.Status.ToString();
        subscription.Tenant.PlanId = subscription.PlanId;
        subscription.Tenant.PlanName = (await _db.Plans.FindAsync(subscription.PlanId))?.Name ?? subscription.Tenant.PlanName;
        _db.PlatformAuditLogs.Add(new PlatformAuditLog
        {
            TenantId = subscription.TenantId, Action = action, Entity = nameof(Subscription),
            EntityId = subscription.SubscriptionId.ToString(), UserId = User.Identity?.Name ?? "unknown",
            Details = $"Subscription {action.ToLowerInvariant()}."
        });
        await _db.SaveChangesAsync();
    }

    private IQueryable<SubscriptionDto> Query()
        => _db.Subscriptions.AsNoTracking().Select(s => new SubscriptionDto
        {
            SubscriptionId = s.SubscriptionId, TenantId = s.TenantId, TenantName = s.Tenant.Name,
            PlanId = s.PlanId, PlanCode = s.Plan.Code, PlanName = s.Plan.Name, Status = s.Status,
            StartDate = s.StartDate, RenewalDate = s.RenewalDate, TrialStartDate = s.TrialStartDate,
            TrialEndDate = s.TrialEndDate, TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(s.TrialEndDate, s.Status),
            BillingCycle = s.BillingCycle, Notes = s.Notes
        });
}

[ApiController]
[Route("api/tenant/subscription")]
[Authorize]
public class TenantSubscriptionController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenant _tenant;
    public TenantSubscriptionController(AppDbContext db, ICurrentTenant tenant) { _db = db; _tenant = tenant; }

    [HttpGet]
    public async Task<ActionResult<SubscriptionDto>> Get()
    {
        if (_tenant.TenantId is not int tenantId) return Forbid();
        var result = await _db.Subscriptions.AsNoTracking().Where(s => s.TenantId == tenantId)
            .Select(s => new SubscriptionDto
            {
                SubscriptionId = s.SubscriptionId, TenantId = s.TenantId, TenantName = s.Tenant.Name,
                PlanId = s.PlanId, PlanCode = s.Plan.Code, PlanName = s.Plan.Name, Status = s.Status,
                StartDate = s.StartDate, RenewalDate = s.RenewalDate, TrialStartDate = s.TrialStartDate,
                TrialEndDate = s.TrialEndDate, TrialDaysRemaining = SubscriptionDto.CalculateTrialDaysRemaining(s.TrialEndDate, s.Status),
                BillingCycle = s.BillingCycle, Notes = s.Notes
            }).SingleOrDefaultAsync();
        return result == null ? NotFound() : Ok(result);
    }
}
