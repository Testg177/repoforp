namespace BlazorApp.Application.DTOs.Configuration;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public int MaxFailedLoginAttempts { get; init; } = 5;
    public int LockoutDurationMinutes { get; init; } = 30;
    public int PasswordMinLength { get; init; } = 8;
    public bool RequireSpecialCharacter { get; init; } = true;
    public int SessionTimeoutMinutes { get; init; } = 480;
    public bool EnableRecaptcha { get; init; }
    public string RecaptchaSiteKey { get; init; } = string.Empty;
    public string RecaptchaSecretKey { get; init; } = string.Empty;
}
