using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Web.Services;

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(string recipientUserId, NotificationListItemDto notification, CancellationToken cancellationToken = default)
        => _hubContext.Clients.Group(recipientUserId).SendAsync("ReceiveNotification", notification, cancellationToken);
}
