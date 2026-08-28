using HRSystem.API.Data;
using HRSystem.API.DTOs;
using HRSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRSystem.API.Services;

public sealed class NotificationService : INotificationService
{
    private static readonly HashSet<string> Channels = new(StringComparer.OrdinalIgnoreCase) { "InApp", "Email" };
    private readonly AppDbContext _db;
    public NotificationService(AppDbContext db) => _db = db;

    public async Task<NotificationDto> PublishAsync(CreateNotificationDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EventCode)) throw new ArgumentException("EventCode is required.");
        if (!Channels.Contains(request.Channel)) throw new ArgumentException("Unsupported notification channel.");
        var template = await _db.Set<NotificationTemplate>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.EventCode == request.EventCode && x.Channel == request.Channel && x.IsActive, cancellationToken);
        var item = new Notification
        {
            EventCode = request.EventCode.Trim(),
            UserId = request.UserId,
            RecipientEmail = request.RecipientEmail,
            Channel = request.Channel,
            Subject = request.Subject ?? template?.SubjectTemplate ?? request.EventCode,
            Body = request.Body ?? template?.BodyTemplate ?? request.EventCode
        };
        _db.Set<Notification>().Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(item);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(int userId, string? email, CancellationToken cancellationToken = default)
        => await _db.Set<Notification>().AsNoTracking()
            .Where(x => x.Channel == "InApp" && (x.UserId == userId || (email != null && x.RecipientEmail == email)))
            .OrderByDescending(x => x.CreatedAt).Take(100).Select(x => new NotificationDto
            {
                Id = x.Id, EventCode = x.EventCode, Channel = x.Channel, Subject = x.Subject,
                Body = x.Body, IsRead = x.IsRead, CreatedAt = x.CreatedAt
            }).ToListAsync(cancellationToken);

    public async Task MarkReadAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var item = await _db.Set<Notification>().SingleOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);
        if (item == null) throw new KeyNotFoundException("Notification not found.");
        item.IsRead = true;
        item.ReadAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto Map(Notification x) => new()
    {
        Id = x.Id, EventCode = x.EventCode, Channel = x.Channel, Subject = x.Subject,
        Body = x.Body, IsRead = x.IsRead, CreatedAt = x.CreatedAt
    };
}
