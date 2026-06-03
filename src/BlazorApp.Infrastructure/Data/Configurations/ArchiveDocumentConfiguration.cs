using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class ArchiveDocumentConfiguration : IEntityTypeConfiguration<ArchiveDocument>
{
    public void Configure(EntityTypeBuilder<ArchiveDocument> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Tags).HasColumnType("text[]");

        builder.HasIndex(x => x.Tags)
            .HasMethod("gin")
            .HasDatabaseName("ix_archive_documents_tags_gin");

        builder.HasIndex(x => x.CategoryId)
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("ix_archive_documents_category_active");

        builder.HasIndex(x => x.DocumentDate)
            .HasDatabaseName("ix_archive_documents_date");
    }
}
