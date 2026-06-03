using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace BlazorApp.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmsService> _logger;
    private readonly AppDbContext _db;

    public SmsService(IConfiguration config, ILogger<SmsService> logger, AppDbContext db)
    {
        _config = config;
        _logger = logger;
        _db = db;
    }

    public async Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default)
    {
        TwilioClient.Init(_config["Sms:AccountSid"], _config["Sms:AuthToken"]);
        try
        {
            var msg = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(_config["Sms:FromNumber"]),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber));
            _logger.LogInformation("SMS sent to {Phone}, SID: {Sid}", toPhoneNumber, msg.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", toPhoneNumber);
            throw;
        }
    }

    public async Task QueueAsync(string toPhoneNumber, string message)
    {
        _db.SmsQueue.Add(new SmsQueue
        {
            ToPhoneNumber = toPhoneNumber,
            Message = message
        });
        await _db.SaveChangesAsync();
    }
}
