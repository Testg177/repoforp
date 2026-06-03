using BlazorApp.Application.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class EmailQueueProcessorJob : IJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly ILogger<EmailQueueProcessorJob> _logger;

    public EmailQueueProcessorJob(AppDbContext db, IEmailService email, ILogger<EmailQueueProcessorJob> logger)
    {
        _db = db;
        _email = email;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var pending = await _db.EmailQueue
            .Where(e => e.Status == "Pending" && e.RetryCount < 3)
            .OrderBy(e => e.CreatedAt)
            .Take(50)
            .ToListAsync(context.CancellationToken);

        foreach (var item in pending)
        {
            try
            {
                await _email.SendAsync(item.ToEmail, item.ToName ?? "", item.Subject, item.BodyHtml, context.CancellationToken);
                item.Status = "Sent";
                item.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                item.RetryCount++;
                item.ErrorMessage = ex.Message;
                if (item.RetryCount >= 3)
                    item.Status = "Failed";
                _logger.LogWarning(ex, "Email queue item {Id} failed (retry {Count})", item.Id, item.RetryCount);
            }
        }

        await _db.SaveChangesAsync(context.CancellationToken);
    }
}
