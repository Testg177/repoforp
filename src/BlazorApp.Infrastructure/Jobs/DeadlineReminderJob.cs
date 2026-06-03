using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class DeadlineReminderJob : IJob
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<DeadlineReminderJob> _logger;

    public DeadlineReminderJob(AppDbContext db, INotificationService notifications,
        IEmailService email, IConfiguration config, ILogger<DeadlineReminderJob> logger)
    {
        _db = db;
        _notifications = notifications;
        _email = email;
        _config = config;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var hoursAhead = _config.GetValue<int>("Scheduler:DeadlineReminderHours", 24);
        var threshold = DateTime.UtcNow.AddHours(hoursAhead);

        var upcoming = await _db.FormSubmissions
            .Include(s => s.SubmittedBy)
            .Include(s => s.FormDefinition)
            .Where(s => s.Status == SubmissionStatus.Draft
                        && s.DueDate <= threshold
                        && s.DueDate > DateTime.UtcNow)
            .ToListAsync(context.CancellationToken);

        foreach (var submission in upcoming)
        {
            await _notifications.CreateAsync(
                submission.SubmittedByUserId,
                NotificationType.FormDeadlineReminder,
                $"Przypomnienie: formularz \"{submission.FormDefinition?.Title}\"",
                $"Termin złożenia upływa {submission.DueDate:dd.MM.yyyy HH:mm}.",
                $"/forms/{submission.Id}",
                submission.Id.ToString(),
                nameof(BlazorApp.Domain.Entities.FormSubmission));

            if (submission.SubmittedBy.ReceiveEmailNotifications && !string.IsNullOrEmpty(submission.SubmittedBy.Email))
            {
                await _email.QueueAsync(
                    submission.SubmittedBy.Email,
                    submission.SubmittedBy.FullName,
                    $"Przypomnienie o terminie: {submission.FormDefinition?.Title}",
                    $"<p>Termin złożenia formularza <strong>{submission.FormDefinition?.Title}</strong> upływa {submission.DueDate:dd.MM.yyyy HH:mm}.</p>");
            }
        }

        _logger.LogInformation("DeadlineReminderJob: processed {Count} submissions", upcoming.Count);
    }
}
