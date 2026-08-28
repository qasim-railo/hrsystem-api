using HRSystem.API.DTOs;
using HRSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRSystem.API.Controllers;

[ApiController, Authorize]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) => _notifications = notifications;

    [HttpPost]
    [Authorize(Policy = "Users.Manage")]
    public async Task<ActionResult<NotificationDto>> Publish(CreateNotificationDto request, CancellationToken cancellationToken)
        => Ok(await _notifications.PublishAsync(request, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Get(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("user_id"), out var userId)) return Forbid();
        return Ok(await _notifications.GetForUserAsync(userId, User.FindFirstValue(ClaimTypes.Name), cancellationToken));
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue("user_id"), out var userId)) return Forbid();
        try { await _notifications.MarkReadAsync(id, userId, cancellationToken); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
