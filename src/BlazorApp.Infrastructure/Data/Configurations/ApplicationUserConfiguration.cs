using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(150);
        builder.Property(x => x.Position).HasMaxLength(150);
        builder.Property(x => x.AvatarUrl).HasMaxLength(500);
        builder.Property(x => x.PhoneNumberForSms).HasMaxLength(32);
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.HasIndex(x => x.IsActive);
    }
}
