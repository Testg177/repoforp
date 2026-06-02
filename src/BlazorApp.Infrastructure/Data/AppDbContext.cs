using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

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
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresEnum<SubmissionStatus>();
        builder.HasPostgresEnum<ModuleType>();
        builder.HasPostgresEnum<ScheduleType>();
        builder.HasPostgresEnum<NotificationType>();
        builder.HasPostgresEnum<FieldType>();

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName()!));

            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.GetColumnBaseName()));
            }

            foreach (var key in entity.GetKeys())
            {
                key.SetName(ToSnakeCase(key.GetName()!));
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()!));
            }

            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
            }
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors(false);
    }

    private static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return string.Concat(value.Select((character, index) =>
            index > 0 && char.IsUpper(character)
                ? "_" + character
                : character.ToString())).ToLowerInvariant();
    }
}
