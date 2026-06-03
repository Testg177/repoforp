namespace BlazorApp.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default);
    Task QueueAsync(string toEmail, string toName, string subject, string bodyHtml, string? relatedEntityId = null, string? notificationType = null);
}
