using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
{
    public void Configure(EntityTypeBuilder<EmailQueue> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityAlwaysColumn();
        builder.Property(x => x.ToEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.ToName).HasMaxLength(250);
        builder.Property(x => x.Subject).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RelatedEntityId).HasMaxLength(128);
        builder.Property(x => x.NotificationType).HasMaxLength(128);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(x => x.CreatedAt).HasFilter("status = 'Pending' AND retry_count < 3").HasDatabaseName("ix_email_queue_pending");
    }
}
