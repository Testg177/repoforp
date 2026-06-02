using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Placeholder).HasMaxLength(500);
        builder.Property(x => x.HelpText).HasMaxLength(1000);
        builder.Property(x => x.ValidationRules).HasColumnType("jsonb");
        builder.Property(x => x.Options).HasColumnType("jsonb");
        builder.Property(x => x.ConditionalLogic).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.FormDefinitionId, x.Order }).HasDatabaseName("ix_form_fields_definition_order");
        builder.HasIndex(x => new { x.FormDefinitionId, x.FieldKey }).IsUnique();
    }
}
