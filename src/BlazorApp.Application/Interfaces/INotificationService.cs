using BlazorApp.Domain.Enums;

namespace BlazorApp.Application.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string recipientUserId, NotificationType type, string title, string message,
        string? actionUrl = null, string? relatedEntityId = null, string? relatedEntityType = null);
    Task MarkAsReadAsync(Guid notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task<int> GetUnreadCountAsync(string userId);
}
