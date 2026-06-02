using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class FormScheduleConfiguration : IEntityTypeConfiguration<FormSchedule>
{
    public void Configure(EntityTypeBuilder<FormSchedule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CronExpression).HasMaxLength(128);
        builder.HasIndex(x => new { x.FormDefinitionId, x.Type, x.IsActive });
    }
}
