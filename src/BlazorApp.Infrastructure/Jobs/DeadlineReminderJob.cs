using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

public sealed class DeadlineReminderJob : IJob
{
    private readonly AppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly SchedulerOptions _options;

    public DeadlineReminderJob(AppDbContext dbContext, INotificationService notificationService, IOptions<SchedulerOptions> options)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var deadline = now.AddHours(_options.DeadlineReminderHours);

        var submissions = await _dbContext.FormSubmissions
            .Include(x => x.FormDefinition)
            .Where(x => x.DueDate >= now && x.DueDate <= deadline && (x.Status == SubmissionStatus.Draft || x.Status == SubmissionStatus.RequiresChanges))
            .ToListAsync(context.CancellationToken);

        foreach (var submission in submissions)
        {
            await _notificationService.CreateAsync(
                submission.SubmittedByUserId,
                NotificationType.FormDeadlineReminder,
                "Termin formularza",
                $"Formularz {submission.FormDefinition.Title} wymaga reakcji przed {submission.DueDate:u}.",
                "/forms",
                submission.Id.ToString(),
                nameof(FormSubmission),
                context.CancellationToken);
        }
    }
}
