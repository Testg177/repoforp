using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.HasIndex(x => x.Status)
            .HasFilter("\"Status\" = 'Pending' AND \"RetryCount\" < 3")
            .HasDatabaseName("ix_email_queue_pending");
    }
}

public class SmsQueueConfiguration : IEntityTypeConfiguration<SmsQueue>
{
    public void Configure(EntityTypeBuilder<SmsQueue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
    }
}

public class ArchiveAccessLogConfiguration : IEntityTypeConfiguration<ArchiveAccessLog>
{
    public void Configure(EntityTypeBuilder<ArchiveAccessLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();

        builder.HasOne(x => x.ArchiveDocument)
            .WithMany(d => d.AccessLogs)
            .HasForeignKey(x => x.ArchiveDocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArchiveSettingsConfiguration : IEntityTypeConfiguration<ArchiveSettings>
{
    public void Configure(EntityTypeBuilder<ArchiveSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AllowedExtensions).HasColumnType("text[]");
    }
}

public class UserSubstitutionConfiguration : IEntityTypeConfiguration<UserSubstitution>
{
    public void Configure(EntityTypeBuilder<UserSubstitution> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Principal)
            .WithMany()
            .HasForeignKey(x => x.PrincipalUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Substitute)
            .WithMany()
            .HasForeignKey(x => x.SubstituteUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FormSubmissionVersionConfiguration : IEntityTypeConfiguration<FormSubmissionVersion>
{
    public void Configure(EntityTypeBuilder<FormSubmissionVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldValues).HasColumnType("jsonb");

        builder.HasOne(x => x.FormSubmission)
            .WithMany(s => s.Versions)
            .HasForeignKey(x => x.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FormCommentConfiguration : IEntityTypeConfiguration<FormComment>
{
    public void Configure(EntityTypeBuilder<FormComment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FormSubmission)
            .WithMany(s => s.Comments)
            .HasForeignKey(x => x.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FormAttachmentConfiguration : IEntityTypeConfiguration<FormAttachment>
{
    public void Configure(EntityTypeBuilder<FormAttachment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FormSubmission)
            .WithMany(s => s.Attachments)
            .HasForeignKey(x => x.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ArchiveCategoryConfiguration : IEntityTypeConfiguration<ArchiveCategory>
{
    public void Configure(EntityTypeBuilder<ArchiveCategory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
