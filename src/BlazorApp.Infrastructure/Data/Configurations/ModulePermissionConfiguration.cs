using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp.Infrastructure.Data.Configurations;

public class ModulePermissionConfiguration : IEntityTypeConfiguration<ModulePermission>
{
    public void Configure(EntityTypeBuilder<ModulePermission> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Module).HasConversion<string>();

        builder.HasOne(x => x.User)
            .WithMany(u => u.ModulePermissions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UserId, x.Module })
            .HasDatabaseName("ix_module_permissions_user_module");

        builder.HasIndex(x => new { x.RoleId, x.Module })
            .HasDatabaseName("ix_module_permissions_role_module");
    }
}
