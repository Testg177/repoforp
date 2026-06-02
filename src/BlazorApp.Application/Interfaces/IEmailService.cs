namespace BlazorApp.Application.Interfaces;

public interface IEmailService
{
    Task QueueAsync(string toEmail, string subject, string bodyHtml, string? toName = null, string? relatedEntityId = null, string? notificationType = null, CancellationToken cancellationToken = default);
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
