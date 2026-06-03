using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext db, ILogger<NotificationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CreateAsync(string recipientUserId, NotificationType type, string title, string message,
        string? actionUrl = null, string? relatedEntityId = null, string? relatedEntityType = null)
    {
        _db.Notifications.Add(new Notification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Message = message,
            ActionUrl = actionUrl,
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = relatedEntityType
        });
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId, string userId)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientUserId == userId);
        if (n is not null)
        {
            n.IsRead = true;
            await _db.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await _db.Notifications
            .Where(x => x.RecipientUserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task<int> GetUnreadCountAsync(string userId) =>
        await _db.Notifications
            .CountAsync(x => x.RecipientUserId == userId && !x.IsRead);
}
