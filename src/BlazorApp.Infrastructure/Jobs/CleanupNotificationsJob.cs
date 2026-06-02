using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

public sealed class CleanupNotificationsJob : IJob
{
    private readonly AppDbContext _dbContext;

    public CleanupNotificationsJob(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var threshold = DateTime.UtcNow.AddDays(-90);
        var oldNotifications = await _dbContext.Notifications
            .Where(x => x.CreatedAt < threshold)
            .ToListAsync(context.CancellationToken);

        if (oldNotifications.Count == 0)
        {
            return;
        }

        _dbContext.Notifications.RemoveRange(oldNotifications);
        await _dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
