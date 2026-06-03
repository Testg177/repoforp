using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(500);

        builder.Property(x => x.AssignedRoles).HasColumnType("text[]");
        builder.Property(x => x.AssignedDepartments).HasColumnType("text[]");

        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ix_form_definitions_code");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("ix_form_definitions_active");
    }
}
