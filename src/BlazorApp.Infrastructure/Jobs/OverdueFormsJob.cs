using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

public sealed class OverdueFormsJob : IJob
{
    private readonly AppDbContext _dbContext;

    public OverdueFormsJob(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.UtcNow;
        var submissions = await _dbContext.FormSubmissions
            .Where(x => !x.IsLate && x.DueDate < now && x.Status != SubmissionStatus.Approved && x.Status != SubmissionStatus.Rejected)
            .ToListAsync(context.CancellationToken);

        foreach (var submission in submissions)
        {
            submission.IsLate = true;
        }

        if (submissions.Count > 0)
        {
            await _dbContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}
