using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.Type).HasConversion<string>();

        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead })
            .HasFilter("\"IsRead\" = false")
            .HasDatabaseName("ix_notifications_recipient_unread");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_notifications_created_at");

        builder.HasOne(x => x.Recipient)
            .WithMany()
            .HasForeignKey(x => x.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
