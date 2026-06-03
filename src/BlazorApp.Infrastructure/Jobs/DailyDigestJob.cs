using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class DailyDigestJob : IJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<DailyDigestJob> _logger;

    public DailyDigestJob(AppDbContext db, IEmailService email, ILogger<DailyDigestJob> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var dueToday = await _db.FormSubmissions
            .Include(s => s.SubmittedBy)
            .Include(s => s.FormDefinition)
            .Where(s => s.Status == SubmissionStatus.Draft
                        && s.DueDate >= today && s.DueDate < tomorrow)
            .ToListAsync(context.CancellationToken);

        var byUser = dueToday.GroupBy(s => s.SubmittedBy);

        foreach (var group in byUser)
        {
            var user = group.Key;
            if (!user.ReceiveEmailNotifications || string.IsNullOrEmpty(user.Email)) continue;

            var items = string.Join("", group.Select(s =>
                $"<li><strong>{s.FormDefinition?.Title}</strong> — termin: {s.DueDate:HH:mm}</li>"));

            await _email.QueueAsync(
                user.Email,
                user.FullName,
                $"Dzisiejsze formularze do złożenia — {today:dd.MM.yyyy}",
                $"<p>Dzień dobry {user.FirstName},</p><p>Dziś masz do złożenia:</p><ul>{items}</ul>");
        }

        _logger.LogInformation("DailyDigestJob: sent {Count} digests", byUser.Count());
    }
}
