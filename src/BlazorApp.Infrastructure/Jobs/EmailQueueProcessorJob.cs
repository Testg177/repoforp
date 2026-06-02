using BlazorApp.Application.Interfaces;
using Quartz;

namespace BlazorApp.Infrastructure.Jobs;

public sealed class EmailQueueProcessorJob : IJob
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public EmailQueueProcessorJob(IEmailService emailService, ISmsService smsService)
    {
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _emailService.ProcessPendingAsync(context.CancellationToken);
        await _smsService.ProcessPendingAsync(context.CancellationToken);
    }
}
