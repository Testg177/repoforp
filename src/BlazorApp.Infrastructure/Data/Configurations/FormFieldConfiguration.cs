using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ValidationRules).HasColumnType("jsonb");
        builder.Property(x => x.Options).HasColumnType("jsonb");
        builder.Property(x => x.ConditionalLogic).HasColumnType("jsonb");

        builder.Property(x => x.Type).HasConversion<string>();

        builder.HasIndex(x => new { x.FormDefinitionId, x.Order })
            .HasDatabaseName("ix_form_fields_definition_order");
    }
}
