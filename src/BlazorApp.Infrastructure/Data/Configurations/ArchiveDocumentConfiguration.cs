using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class ArchiveDocumentConfiguration : IEntityTypeConfiguration<ArchiveDocument>
{
    public void Configure(EntityTypeBuilder<ArchiveDocument> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.RelativePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DocumentNumber).HasMaxLength(120);
        builder.Property(x => x.Tags).HasColumnType("text[]");
        builder.Property(x => x.Checksum).HasMaxLength(128);
        builder.HasIndex(x => x.Tags).HasMethod("gin").HasDatabaseName("ix_archive_documents_tags_gin");
        builder.HasIndex(x => x.CategoryId).HasFilter("is_deleted = false").HasDatabaseName("ix_archive_documents_category_active");
        builder.HasIndex(x => x.DocumentDate).HasDatabaseName("ix_archive_documents_date");
    }
}
