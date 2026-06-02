using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormSubmissionVersionConfiguration : IEntityTypeConfiguration<FormSubmissionVersion>
{
    public void Configure(EntityTypeBuilder<FormSubmissionVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.FieldValues).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SavedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.ChangeComment).HasMaxLength(1000);
        builder.HasIndex(x => new { x.FormSubmissionId, x.VersionNumber }).IsUnique();
    }
}
