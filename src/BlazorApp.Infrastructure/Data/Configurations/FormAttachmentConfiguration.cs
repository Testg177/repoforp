using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormAttachmentConfiguration : IEntityTypeConfiguration<FormAttachment>
{
    public void Configure(EntityTypeBuilder<FormAttachment> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UploadedByUserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => x.FormSubmissionId);
    }
}
