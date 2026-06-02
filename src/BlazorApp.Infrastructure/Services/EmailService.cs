using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;

namespace BlazorApp.Infrastructure.Services;

public sealed class EmailService : IEmailService
{
    private readonly AppDbContext _dbContext;
    private readonly EmailOptions _options;

    public EmailService(AppDbContext dbContext, IOptions<EmailOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task QueueAsync(string toEmail, string subject, string bodyHtml, string? toName = null, string? relatedEntityId = null, string? notificationType = null, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailQueue.Add(new EmailQueue
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            BodyHtml = bodyHtml,
            Status = "Pending",
            RelatedEntityId = relatedEntityId,
            NotificationType = notificationType
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.EmailQueue
            .Where(x => x.Status == "Pending" && x.RetryCount < 3)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var item in batch)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_options.SmtpHost) || string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
                {
                    throw new InvalidOperationException("SMTP is not configured.");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
                message.To.Add(new MailboxAddress(item.ToName ?? item.ToEmail, item.ToEmail));
                message.Subject = item.Subject;
                message.Body = new BodyBuilder { HtmlBody = item.BodyHtml }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, _options.UseSsl, cancellationToken);
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                await client.SendAsync(message, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                item.Status = "Sent";
                item.SentAt = DateTime.UtcNow;
                item.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                item.RetryCount += 1;
                item.Status = item.RetryCount >= 3 ? "Failed" : "Pending";
                item.ErrorMessage = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return batch.Count;
    }
}
