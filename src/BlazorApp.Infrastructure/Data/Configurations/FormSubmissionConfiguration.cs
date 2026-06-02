using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.FieldValues).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);

        builder.HasIndex(x => x.Status).HasDatabaseName("ix_form_submissions_status");
        builder.HasIndex(x => x.DueDate).HasDatabaseName("ix_form_submissions_due_date");
        builder.HasIndex(x => x.SubmittedByUserId).HasDatabaseName("ix_form_submissions_submitted_by");
        builder.HasIndex(x => new { x.FormDefinitionId, x.Status }).HasDatabaseName("ix_form_submissions_form_status");
        builder.HasIndex(x => x.FieldValues).HasMethod("gin").HasDatabaseName("ix_form_submissions_field_values_gin");

        builder.HasOne(x => x.FormDefinition)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.FormDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubmittedBy)
            .WithMany()
            .HasForeignKey(x => x.SubmittedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RejectionCategory)
            .WithMany()
            .HasForeignKey(x => x.RejectionCategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
