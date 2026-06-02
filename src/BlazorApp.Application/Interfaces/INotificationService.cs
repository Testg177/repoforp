using BlazorApp.Application.DTOs;
using BlazorApp.Domain.Enums;

namespace BlazorApp.Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationListItemDto>> GetUnreadAsync(CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task CreateAsync(string recipientUserId, NotificationType type, string title, string message, string? actionUrl = null, string? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default);
}
