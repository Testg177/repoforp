namespace BlazorApp.Application.DTOs.Configuration;

public sealed class SmsOptions
{
    public const string SectionName = "Sms";

    public string Provider { get; init; } = "Twilio";
    public string AccountSid { get; init; } = string.Empty;
    public string AuthToken { get; init; } = string.Empty;
    public string FromNumber { get; init; } = string.Empty;
}
