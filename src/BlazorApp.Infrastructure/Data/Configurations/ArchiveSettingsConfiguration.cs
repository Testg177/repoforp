using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class ArchiveSettingsConfiguration : IEntityTypeConfiguration<ArchiveSettings>
{
    public void Configure(EntityTypeBuilder<ArchiveSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.AllowedExtensions).HasColumnType("text[]");
        builder.Property(x => x.IndexPath).HasMaxLength(1000);
        builder.Property(x => x.ModifiedBy).HasMaxLength(450);
    }
}
