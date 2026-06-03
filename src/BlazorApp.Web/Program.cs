using BlazorApp.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using BlazorApp.Application.Validators;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Jobs;
using BlazorApp.Infrastructure.Services;
using BlazorApp.Web.Components;
using BlazorApp.Web.Components.Account;
using BlazorApp.Web.Hubs;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Npgsql;
using Quartz;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .Enrich.FromLogContext());

// --- PostgreSQL via EF Core ---
Action<DbContextOptionsBuilder> dbOptions = options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql =>
        {
            npgsql.MigrationsAssembly("BlazorApp.Infrastructure");
            npgsql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorCodesToAdd: null);
            npgsql.CommandTimeout(30);
        });
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
};
builder.Services.AddDbContext<AppDbContext>(dbOptions);
builder.Services.AddDbContextFactory<AppDbContext>(dbOptions, ServiceLifetime.Scoped);

// --- ASP.NET Core Identity ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// --- Claims transformer (loads module permissions as claims) ---
builder.Services.AddScoped<IClaimsTransformation, BlazorApp.Infrastructure.Services.ModulePermissionClaimsTransformer>();

// --- Authorization policies ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewForms", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.Forms.View", "true")
        || ctx.User.IsInRole("SuperAdmin") || ctx.User.IsInRole("Admin")));

    options.AddPolicy("CanApproveForms", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.Forms.Approve", "true")
        || ctx.User.IsInRole("Manager") || ctx.User.IsInRole("Admin") || ctx.User.IsInRole("SuperAdmin")));

    options.AddPolicy("CanViewArchive", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.Archive.View", "true")
        || ctx.User.IsInRole("SuperAdmin") || ctx.User.IsInRole("Admin")));

    options.AddPolicy("CanViewReports", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.Reports.View", "true")
        || ctx.User.IsInRole("SuperAdmin") || ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Manager")));

    options.AddPolicy("CanManageUsers", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.UserManagement.View", "true")
        || ctx.User.IsInRole("Admin") || ctx.User.IsInRole("SuperAdmin")));

    options.AddPolicy("CanViewAudit", p => p.RequireAssertion(ctx =>
        ctx.User.HasClaim("Module.Audit.View", "true")
        || ctx.User.IsInRole("Admin") || ctx.User.IsInRole("SuperAdmin") || ctx.User.IsInRole("Auditor")));
});

// --- Data Protection stored in PostgreSQL ---
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("BlazorApp");

// --- Quartz with PostgreSQL persistent store ---
builder.Services.AddQuartz(q =>
{
    q.UsePersistentStore(store =>
    {
        store.UsePostgres(pg =>
        {
            pg.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
            pg.TablePrefix = "qrtz_";
        });
        store.UseNewtonsoftJsonSerializer();
        store.UseClustering();
    });

    // Daily deadline reminder — every hour
    var reminderKey = new JobKey("DeadlineReminderJob");
    q.AddJob<DeadlineReminderJob>(opts => opts.WithIdentity(reminderKey));
    q.AddTrigger(opts => opts.ForJob(reminderKey)
        .WithIdentity("DeadlineReminderTrigger")
        .WithCronSchedule("0 0 * * * ?"));

    // Mark overdue — every 15 minutes
    var overdueKey = new JobKey("OverdueFormsJob");
    q.AddJob<OverdueFormsJob>(opts => opts.WithIdentity(overdueKey));
    q.AddTrigger(opts => opts.ForJob(overdueKey)
        .WithIdentity("OverdueFormsTrigger")
        .WithCronSchedule("0 0/15 * * * ?"));

    // Daily digest — 7:00 AM
    var digestKey = new JobKey("DailyDigestJob");
    q.AddJob<DailyDigestJob>(opts => opts.WithIdentity(digestKey));
    q.AddTrigger(opts => opts.ForJob(digestKey)
        .WithIdentity("DailyDigestTrigger")
        .WithCronSchedule("0 0 7 * * ?"));

    // Email queue processor — every 2 minutes
    var emailKey = new JobKey("EmailQueueProcessorJob");
    q.AddJob<EmailQueueProcessorJob>(opts => opts.WithIdentity(emailKey));
    q.AddTrigger(opts => opts.ForJob(emailKey)
        .WithIdentity("EmailQueueTrigger")
        .WithCronSchedule("0 0/2 * * * ?"));

    // Cleanup notifications — daily at midnight
    var cleanupKey = new JobKey("CleanupNotificationsJob");
    q.AddJob<CleanupNotificationsJob>(opts => opts.WithIdentity(cleanupKey));
    q.AddTrigger(opts => opts.ForJob(cleanupKey)
        .WithIdentity("CleanupTrigger")
        .WithCronSchedule("0 0 0 * * ?"));
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// --- SignalR ---
builder.Services.AddSignalR();

// --- MudBlazor ---
builder.Services.AddMudServices();

// --- FluentValidation ---
builder.Services.AddValidatorsFromAssemblyContaining<CreateFormSubmissionValidator>();

// --- Application services ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IArchiveFileService, ArchiveFileService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// --- Blazor ---
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await InitQuartzSchemaAsync(db);
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapAdditionalIdentityEndpoints();

app.Run();

static async Task InitQuartzSchemaAsync(AppDbContext db)
{
    const string sql = """
        CREATE TABLE IF NOT EXISTS qrtz_job_details (
            sched_name TEXT NOT NULL,
            job_name   TEXT NOT NULL,
            job_group  TEXT NOT NULL,
            description TEXT NULL,
            job_class_name TEXT NOT NULL,
            is_durable BOOL NOT NULL,
            is_nonconcurrent BOOL NOT NULL,
            is_update_data BOOL NOT NULL,
            requests_recovery BOOL NOT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name, job_name, job_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_triggers (
            sched_name    TEXT NOT NULL,
            trigger_name  TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            job_name      TEXT NOT NULL,
            job_group     TEXT NOT NULL,
            description TEXT NULL,
            next_fire_time BIGINT NULL,
            prev_fire_time BIGINT NULL,
            priority INTEGER NULL,
            trigger_state TEXT NOT NULL,
            misfire_orig_fire_time BIGINT NULL,
            trigger_type  TEXT NOT NULL,
            start_time BIGINT NOT NULL,
            end_time   BIGINT NULL,
            calendar_name TEXT NULL,
            misfire_instr SMALLINT NULL,
            job_data BYTEA NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, job_name, job_group)
                REFERENCES qrtz_job_details (sched_name, job_name, job_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_simple_triggers (
            sched_name      TEXT NOT NULL,
            trigger_name    TEXT NOT NULL,
            trigger_group   TEXT NOT NULL,
            repeat_count    BIGINT NOT NULL,
            repeat_interval BIGINT NOT NULL,
            times_triggered BIGINT NOT NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_simprop_triggers (
            sched_name    TEXT NOT NULL,
            trigger_name  TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            str_prop_1 TEXT NULL, str_prop_2 TEXT NULL, str_prop_3 TEXT NULL,
            int_prop_1 INT NULL,  int_prop_2 INT NULL,
            long_prop_1 BIGINT NULL, long_prop_2 BIGINT NULL,
            dec_prop_1 NUMERIC(13,4) NULL, dec_prop_2 NUMERIC(13,4) NULL,
            bool_prop_1 BOOL NULL, bool_prop_2 BOOL NULL,
            time_zone_id TEXT NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_cron_triggers (
            sched_name      TEXT NOT NULL,
            trigger_name    TEXT NOT NULL,
            trigger_group   TEXT NOT NULL,
            cron_expression TEXT NOT NULL,
            time_zone_id    TEXT,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_blob_triggers (
            sched_name    TEXT NOT NULL,
            trigger_name  TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            blob_data BYTEA NULL,
            PRIMARY KEY (sched_name, trigger_name, trigger_group),
            FOREIGN KEY (sched_name, trigger_name, trigger_group)
                REFERENCES qrtz_triggers (sched_name, trigger_name, trigger_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_calendars (
            sched_name    TEXT NOT NULL,
            calendar_name TEXT NOT NULL,
            calendar BYTEA NOT NULL,
            PRIMARY KEY (sched_name, calendar_name)
        );
        CREATE TABLE IF NOT EXISTS qrtz_paused_trigger_grps (
            sched_name    TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            PRIMARY KEY (sched_name, trigger_group)
        );
        CREATE TABLE IF NOT EXISTS qrtz_fired_triggers (
            sched_name    TEXT NOT NULL,
            entry_id      TEXT NOT NULL,
            trigger_name  TEXT NOT NULL,
            trigger_group TEXT NOT NULL,
            instance_name TEXT NOT NULL,
            fired_time    BIGINT NOT NULL,
            sched_time    BIGINT NOT NULL,
            priority      INTEGER NOT NULL,
            state         TEXT NOT NULL,
            job_name  TEXT NULL,
            job_group TEXT NULL,
            is_nonconcurrent BOOL NULL,
            requests_recovery BOOL NULL,
            PRIMARY KEY (sched_name, entry_id)
        );
        CREATE TABLE IF NOT EXISTS qrtz_scheduler_state (
            sched_name        TEXT NOT NULL,
            instance_name     TEXT NOT NULL,
            last_checkin_time BIGINT NOT NULL,
            checkin_interval  BIGINT NOT NULL,
            PRIMARY KEY (sched_name, instance_name)
        );
        CREATE TABLE IF NOT EXISTS qrtz_locks (
            sched_name TEXT NOT NULL,
            lock_name  TEXT NOT NULL,
            PRIMARY KEY (sched_name, lock_name)
        );
        INSERT INTO qrtz_locks VALUES('QuartzScheduler','TRIGGER_ACCESS')  ON CONFLICT DO NOTHING;
        INSERT INTO qrtz_locks VALUES('QuartzScheduler','JOB_ACCESS')      ON CONFLICT DO NOTHING;
        INSERT INTO qrtz_locks VALUES('QuartzScheduler','CALENDAR_ACCESS') ON CONFLICT DO NOTHING;
        INSERT INTO qrtz_locks VALUES('QuartzScheduler','STATE_ACCESS')    ON CONFLICT DO NOTHING;
        INSERT INTO qrtz_locks VALUES('QuartzScheduler','MISFIRE_ACCESS')  ON CONFLICT DO NOTHING;
        ALTER TABLE qrtz_triggers ADD COLUMN IF NOT EXISTS misfire_orig_fire_time BIGINT NULL;
        """;

    await using var conn = new NpgsqlConnection(db.Database.GetConnectionString());
    await conn.OpenAsync();
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    await cmd.ExecuteNonQueryAsync();
}
