namespace BlazorApp.Application.DTOs.Configuration;

public sealed class AppSettingsOptions
{
    public const string SectionName = "AppSettings";

    public string AppName { get; init; } = "System Zarzadzania";
    public string CompanyName { get; init; } = string.Empty;
    public string LogoPath { get; init; } = string.Empty;
    public string DefaultCulture { get; init; } = "pl-PL";
    public string TimeZone { get; init; } = "Europe/Warsaw";
}
