using System.Security.Claims;

namespace HRSystem.API.Tenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CurrentTenant currentTenant)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantClaim = context.User.FindFirstValue("tenant_id");
            if (!int.TryParse(tenantClaim, out var tenantId) || tenantId <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("The authenticated user is not assigned to a tenant.");
                return;
            }

            currentTenant.SetTenant(tenantId);
        }

        await _next(context);
    }
}
