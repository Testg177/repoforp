using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.RecipientUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ActionUrl).HasMaxLength(500);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(128);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(128);
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead }).HasFilter("is_read = false").HasDatabaseName("ix_notifications_recipient_unread");
        builder.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_notifications_created_at");
    }
}
