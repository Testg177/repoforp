using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Category).HasMaxLength(100);
        builder.Property(x => x.CreatedByUserId).HasMaxLength(450);
        builder.Property(x => x.AssignedRoles).HasColumnType("text[]");
        builder.Property(x => x.AssignedDepartments).HasColumnType("text[]");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.Category, x.IsActive });
    }
}
