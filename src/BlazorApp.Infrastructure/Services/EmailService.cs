using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BlazorApp.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly AppDbContext _db;

    public EmailService(IConfiguration config, ILogger<EmailService> logger, AppDbContext db)
    {
        _config = config;
        _logger = logger;
        _db = db;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:FromName"] ?? "System",
            _config["Email:FromAddress"] ?? "noreply@firma.pl"));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = bodyHtml };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(
                _config["Email:SmtpHost"] ?? "localhost",
                int.Parse(_config["Email:SmtpPort"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(_config["Email:Username"] ?? "", _config["Email:Password"] ?? "", ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }

    public async Task QueueAsync(string toEmail, string toName, string subject, string bodyHtml,
        string? relatedEntityId = null, string? notificationType = null)
    {
        _db.EmailQueue.Add(new EmailQueue
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            BodyHtml = bodyHtml,
            RelatedEntityId = relatedEntityId,
            NotificationType = notificationType
        });
        await _db.SaveChangesAsync();
    }
}
