using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ModulePermission> ModulePermissions => Set<ModulePermission>();
    public DbSet<UserSubstitution> UserSubstitutions => Set<UserSubstitution>();

    public DbSet<FormDefinition> FormDefinitions => Set<FormDefinition>();
    public DbSet<FormField> FormFields => Set<FormField>();
    public DbSet<FormSchedule> FormSchedules => Set<FormSchedule>();
    public DbSet<FormSubmission> FormSubmissions => Set<FormSubmission>();
    public DbSet<FormAttachment> FormAttachments => Set<FormAttachment>();
    public DbSet<FormComment> FormComments => Set<FormComment>();
    public DbSet<FormSubmissionVersion> FormSubmissionVersions => Set<FormSubmissionVersion>();
    public DbSet<RejectionCategory> RejectionCategories => Set<RejectionCategory>();

    public DbSet<ArchiveDocument> ArchiveDocuments => Set<ArchiveDocument>();
    public DbSet<ArchiveCategory> ArchiveCategories => Set<ArchiveCategory>();
    public DbSet<ArchiveSettings> ArchiveSettings => Set<ArchiveSettings>();
    public DbSet<ArchiveAccessLog> ArchiveAccessLogs => Set<ArchiveAccessLog>();

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailQueue> EmailQueue => Set<EmailQueue>();
    public DbSet<SmsQueue> SmsQueue => Set<SmsQueue>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    // IDataProtectionKeyContext
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
