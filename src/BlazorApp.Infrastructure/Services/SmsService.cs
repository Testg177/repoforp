using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BlazorApp.Infrastructure.Services;

public sealed class SmsService : ISmsService
{
    private readonly AppDbContext _dbContext;
    private readonly SmsOptions _options;

    public SmsService(AppDbContext dbContext, IOptions<SmsOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task QueueAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default)
    {
        _dbContext.SmsQueue.Add(new SmsQueue
        {
            ToPhoneNumber = toPhoneNumber,
            Message = message,
            Status = "Pending"
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.SmsQueue
            .Where(x => x.Status == "Pending" && x.RetryCount < 3)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var item in batch)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_options.AccountSid) || string.IsNullOrWhiteSpace(_options.AuthToken) || string.IsNullOrWhiteSpace(_options.FromNumber))
                {
                    throw new InvalidOperationException("SMS provider is not configured.");
                }

                TwilioClient.Init(_options.AccountSid, _options.AuthToken);
                var result = await MessageResource.CreateAsync(
                    to: new PhoneNumber(item.ToPhoneNumber),
                    from: new PhoneNumber(_options.FromNumber),
                    body: item.Message);

                item.Status = "Sent";
                item.SentAt = DateTime.UtcNow;
                item.ExternalMessageId = result.Sid;
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
