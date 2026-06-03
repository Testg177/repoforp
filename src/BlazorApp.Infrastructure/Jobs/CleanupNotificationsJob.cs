using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class CleanupNotificationsJob : IJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<CleanupNotificationsJob> _logger;

    public CleanupNotificationsJob(AppDbContext db, ILogger<CleanupNotificationsJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var cutoff = DateTime.UtcNow.AddDays(-90);
        var deleted = await _db.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ExecuteDeleteAsync(context.CancellationToken);

        _logger.LogInformation("CleanupNotificationsJob: deleted {Count} old notifications", deleted);
    }
}
