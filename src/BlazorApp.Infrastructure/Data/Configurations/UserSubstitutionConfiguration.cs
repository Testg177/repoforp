using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public sealed class UserSubstitutionConfiguration : IEntityTypeConfiguration<UserSubstitution>
{
    public void Configure(EntityTypeBuilder<UserSubstitution> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Scope).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.PrincipalUserId, x.SubstituteUserId, x.ValidFrom, x.ValidTo });

        builder.HasOne(x => x.Principal)
            .WithMany()
            .HasForeignKey(x => x.PrincipalUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Substitute)
            .WithMany()
            .HasForeignKey(x => x.SubstituteUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
