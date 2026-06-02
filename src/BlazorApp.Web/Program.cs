using ApexCharts;
using Blazored.LocalStorage;
using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Extensions;
using BlazorApp.Infrastructure.Jobs;
using BlazorApp.Web.Areas.Identity;
using BlazorApp.Web.Hubs;
using BlazorApp.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Npgsql;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var securityOptions = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>() ?? new SecurityOptions();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Default connection string is not configured.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<SubmissionStatus>();
dataSourceBuilder.MapEnum<ModuleType>();
dataSourceBuilder.MapEnum<ScheduleType>();
dataSourceBuilder.MapEnum<NotificationType>();
dataSourceBuilder.MapEnum<FieldType>();
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: connectionString,
        tableName: "serilog_logs",
        needAutoCreateTable: true));

builder.Services.AddApplicationInfrastructure(builder.Configuration);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(dataSource, npgsql =>
    {
        npgsql.MigrationsAssembly("BlazorApp.Infrastructure");
        npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
        npgsql.CommandTimeout(30);
    });

    options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = securityOptions.PasswordMinLength;
    options.Password.RequireNonAlphanumeric = securityOptions.RequireSpecialCharacter;
    options.Lockout.MaxFailedAccessAttempts = securityOptions.MaxFailedLoginAttempts;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(securityOptions.LockoutDurationMinutes);
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(securityOptions.SessionTimeoutMinutes);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewForms", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Module.Forms.View", "true") ||
        context.User.IsInRole("Manager") ||
        context.User.IsInRole("Admin") ||
        context.User.IsInRole("SuperAdmin") ||
        context.User.IsInRole("Employee")));

    options.AddPolicy("CanApproveForms", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Module.Forms.Approve", "true") ||
        context.User.IsInRole("Manager") ||
        context.User.IsInRole("Admin") ||
        context.User.IsInRole("SuperAdmin")));

    options.AddPolicy("CanViewArchive", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Module.Archive.View", "true") ||
        context.User.IsInRole("Admin") ||
        context.User.IsInRole("SuperAdmin")));

    options.AddPolicy("CanViewReports", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Module.Reports.View", "true") ||
        context.User.IsInRole("Manager") ||
        context.User.IsInRole("Admin") ||
        context.User.IsInRole("SuperAdmin") ||
        context.User.IsInRole("Auditor")));

    options.AddPolicy("CanManageUsers", policy => policy.RequireRole("Admin", "SuperAdmin"));
});

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("BlazorApp");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddApexCharts();
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UseProperties = true;
        store.RetryInterval = TimeSpan.FromSeconds(15);
        store.UsePostgres(postgres =>
        {
            postgres.ConnectionString = connectionString;
            postgres.TablePrefix = "qrtz_";
        });
        store.UseClustering();
        store.UseSystemTextJsonSerializer();
    });

    var deadlineJobKey = new JobKey(nameof(DeadlineReminderJob));
    q.AddJob<DeadlineReminderJob>(opts => opts.WithIdentity(deadlineJobKey));
    q.AddTrigger(opts => opts.ForJob(deadlineJobKey).WithIdentity($"{nameof(DeadlineReminderJob)}-trigger").WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromHours(1)).RepeatForever()));

    var overdueJobKey = new JobKey(nameof(OverdueFormsJob));
    q.AddJob<OverdueFormsJob>(opts => opts.WithIdentity(overdueJobKey));
    q.AddTrigger(opts => opts.ForJob(overdueJobKey).WithIdentity($"{nameof(OverdueFormsJob)}-trigger").WithCronSchedule("0 0 6 * * ?"));

    var digestJobKey = new JobKey(nameof(DailyDigestJob));
    q.AddJob<DailyDigestJob>(opts => opts.WithIdentity(digestJobKey));
    q.AddTrigger(opts => opts.ForJob(digestJobKey).WithIdentity($"{nameof(DailyDigestJob)}-trigger").WithCronSchedule("0 0 7 * * ?"));

    var emailJobKey = new JobKey(nameof(EmailQueueProcessorJob));
    q.AddJob<EmailQueueProcessorJob>(opts => opts.WithIdentity(emailJobKey));
    q.AddTrigger(opts => opts.ForJob(emailJobKey).WithIdentity($"{nameof(EmailQueueProcessorJob)}-trigger").WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(1)).RepeatForever()));

    var cleanupJobKey = new JobKey(nameof(CleanupNotificationsJob));
    q.AddJob<CleanupNotificationsJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts.ForJob(cleanupJobKey).WithIdentity($"{nameof(CleanupNotificationsJob)}-trigger").WithCronSchedule("0 0 2 * * ?"));
});
builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedAsync();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
