using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using BlazorApp.Application.Validators;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Security;
using BlazorApp.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorApp.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettingsOptions>(configuration.GetSection(AppSettingsOptions.SectionName));
        services.Configure<AdminBootstrapOptions>(configuration.GetSection(AdminBootstrapOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<SmsOptions>(configuration.GetSection(SmsOptions.SectionName));
        services.Configure<ArchiveOptions>(configuration.GetSection(ArchiveOptions.SectionName));
        services.Configure<SchedulerOptions>(configuration.GetSection(SchedulerOptions.SectionName));
        services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.TryAddScoped<INotificationPublisher, NullNotificationPublisher>();
        services.AddScoped<IClaimsTransformation, ModulePermissionClaimsTransformation>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFormsService, FormsService>();
        services.AddScoped<IArchiveService, ArchiveService>();
        services.AddScoped<IReportsService, ReportsService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IArchiveFileService, ArchiveFileService>();
        services.AddScoped<IUserDirectoryService, UserDirectoryService>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }

    public static async Task SeedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
}
