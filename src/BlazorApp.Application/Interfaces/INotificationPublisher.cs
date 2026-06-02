using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface INotificationPublisher
{
    Task PublishAsync(string recipientUserId, NotificationListItemDto notification, CancellationToken cancellationToken = default);
}
