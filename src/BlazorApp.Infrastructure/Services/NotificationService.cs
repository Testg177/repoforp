using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly INotificationPublisher _publisher;

    public NotificationService(AppDbContext dbContext, ICurrentUserAccessor currentUserAccessor, INotificationPublisher publisher)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _publisher = publisher;
    }

    public async Task<IReadOnlyList<NotificationListItemDto>> GetUnreadAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return [];
        }

        return await _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.RecipientUserId == _currentUserAccessor.UserId && !x.IsRead)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new NotificationListItemDto
            {
                Id = x.Id,
                Type = x.Type,
                Title = x.Title,
                Message = x.Message,
                ActionUrl = x.ActionUrl,
                IsRead = x.IsRead,
                CreatedAtUtc = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return;
        }

        if (_currentUserAccessor.IsAuthenticated && notification.RecipientUserId != _currentUserAccessor.UserId)
        {
            return;
        }

        notification.IsRead = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateAsync(string recipientUserId, NotificationType type, string title, string message, string? actionUrl = null, string? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _publisher.PublishAsync(recipientUserId, new NotificationListItemDto
        {
            Id = notification.Id,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            ActionUrl = notification.ActionUrl,
            IsRead = notification.IsRead,
            CreatedAtUtc = notification.CreatedAt
        }, cancellationToken);
    }
}
