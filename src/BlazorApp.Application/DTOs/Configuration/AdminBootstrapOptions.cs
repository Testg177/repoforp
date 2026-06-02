namespace BlazorApp.Application.DTOs.Configuration;

public sealed class AdminBootstrapOptions
{
    public const string SectionName = "AdminBootstrap";

    public string SuperAdminEmail { get; init; } = "superadmin@firma.pl";
    public string SuperAdminPassword { get; init; } = string.Empty;
}
