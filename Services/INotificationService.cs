using HRSystem.API.DTOs;

namespace HRSystem.API.Services;

public interface INotificationService
{
    Task<NotificationDto> PublishAsync(CreateNotificationDto request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(int userId, string? email, CancellationToken cancellationToken = default);
    Task MarkReadAsync(int id, int userId, CancellationToken cancellationToken = default);
}
