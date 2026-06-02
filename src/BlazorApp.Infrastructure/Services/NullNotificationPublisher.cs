using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;

namespace BlazorApp.Infrastructure.Services;

public sealed class NullNotificationPublisher : INotificationPublisher
{
    public Task PublishAsync(string recipientUserId, NotificationListItemDto notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
