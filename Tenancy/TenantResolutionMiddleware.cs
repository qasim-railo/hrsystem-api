using System.Security.Claims;
using System.Text.Json;
using HRSystem.API.Data;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Tenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext db, CurrentTenant currentTenant)
    {
        currentTenant.Clear();

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var isPlatformAdmin = context.User.HasClaim("permission", "Platform.Tenants") || context.User.IsInRole("PeopleOS Super Admin");
        if (isPlatformAdmin)
        {
            currentTenant.SetPlatformAdmin();
            _logger.LogInformation("Platform admin context resolved without tenant assignment for {Path}.", context.Request.Path);
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirstValue("user_id");
        if (!int.TryParse(userIdClaim, out var userId) || userId <= 0)
        {
            _logger.LogWarning("Rejected authenticated request without a valid user_id claim for path {Path}.", context.Request.Path);
            await RejectRequestAsync(context, "The authenticated user is not assigned to a valid PeopleOS account.");
            return;
        }

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("Rejected request for inactive or unknown user {UserId} on path {Path}.", userId, context.Request.Path);
            await RejectRequestAsync(context, "The authenticated user is not active or no longer exists.");
            return;
        }

        var suppliedTenantId = await TryReadSuppliedTenantIdAsync(context);
        var tenantClaim = context.User.FindFirstValue("tenant_id");
        if (int.TryParse(tenantClaim, out var jwtTenantId) && jwtTenantId != user.TenantId)
        {
            _logger.LogWarning("Rejected suspicious tenant mismatch for user {UserId}: JWT tenant {JwtTenantId} does not match user tenant {UserTenantId}.", userId, jwtTenantId, user.TenantId);
            await RejectRequestAsync(context, "The tenant identity in the request is invalid.");
            return;
        }

        if (suppliedTenantId is int requestedTenantId && requestedTenantId != user.TenantId)
        {
            _logger.LogWarning("Ignored untrusted tenantId value {RequestedTenantId} for user {UserId}.", requestedTenantId, userId);
            await RejectRequestAsync(context, "Tenant context does not match the authenticated user.");
            return;
        }

        var tenant = await db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.TenantId == user.TenantId);
        if (tenant == null)
        {
            _logger.LogWarning("Rejected request for missing tenant {TenantId} on user {UserId}.", user.TenantId, userId);
            await RejectRequestAsync(context, "The tenant associated with this user no longer exists.");
            return;
        }

        if (IsTenantBlocked(tenant))
        {
            _logger.LogWarning("Rejected request for blocked tenant {TenantId} ({TenantStatus}/{LifecycleStatus}) for user {UserId}.", tenant.TenantId, tenant.Status, tenant.LifecycleStatus, userId);
            await RejectRequestAsync(context, "This tenant is not available for authenticated access.");
            return;
        }

        var subscription = await db.Subscriptions.AsNoTracking()
            .Where(s => s.TenantId == tenant.TenantId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (subscription != null && IsSubscriptionBlocked(subscription.Status))
        {
            _logger.LogWarning("Rejected request for tenant {TenantId} because its active subscription is {Status}.", tenant.TenantId, subscription.Status);
            await RejectRequestAsync(context, "This tenant subscription is not active.");
            return;
        }

        currentTenant.SetTenant(user.TenantId);
        await _next(context);
    }

    private static async Task RejectRequestAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync(message);
    }

    private static bool IsTenantBlocked(Tenant tenant)
        => tenant.Status.Equals("Suspended", StringComparison.OrdinalIgnoreCase)
            || tenant.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase)
            || tenant.Status.Equals("Deleted", StringComparison.OrdinalIgnoreCase)
            || tenant.LifecycleStatus.Equals("Suspended", StringComparison.OrdinalIgnoreCase)
            || tenant.LifecycleStatus.Equals("Archived", StringComparison.OrdinalIgnoreCase)
            || tenant.LifecycleStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
            || tenant.LifecycleStatus.Equals("Expired", StringComparison.OrdinalIgnoreCase);

    private static bool IsSubscriptionBlocked(SubscriptionStatus status)
        => status is SubscriptionStatus.PastDue or SubscriptionStatus.Suspended or SubscriptionStatus.Cancelled or SubscriptionStatus.Expired;

    private static async Task<int?> TryReadSuppliedTenantIdAsync(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("tenantId", out var tenantQuery) && int.TryParse(tenantQuery, out var queryTenantId))
            return queryTenantId;

        if (context.Request.ContentLength is <= 0)
            return null;

        if (!context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
            return null;

        context.Request.EnableBuffering();
        context.Request.Body.Position = 0;

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var bodyText = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(bodyText))
            return null;

        try
        {
            using var document = JsonDocument.Parse(bodyText);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!document.RootElement.TryGetProperty("tenantId", out var tenantElement))
                return null;

            return tenantElement.TryGetInt32(out var bodyTenantId) ? bodyTenantId : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
