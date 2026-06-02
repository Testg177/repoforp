using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RecipientUserId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }

    public ApplicationUser Recipient { get; set; } = null!;
}
