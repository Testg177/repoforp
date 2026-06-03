using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class OverdueFormsJob : IJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<OverdueFormsJob> _logger;

    public OverdueFormsJob(AppDbContext db, ILogger<OverdueFormsJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var updated = await _db.FormSubmissions
            .Where(s => s.Status == SubmissionStatus.Draft
                        && s.DueDate < DateTime.UtcNow
                        && !s.IsLate)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsLate, true),
                context.CancellationToken);

        _logger.LogInformation("OverdueFormsJob: marked {Count} submissions as late", updated);
    }
}
