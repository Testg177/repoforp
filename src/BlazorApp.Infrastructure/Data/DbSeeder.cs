using BlazorApp.Application.DTOs.Configuration;
using BlazorApp.Domain.Entities;
using BlazorApp.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BlazorApp.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var adminBootstrap = services.GetRequiredService<IOptions<AdminBootstrapOptions>>().Value;

        string[] roles = ["SuperAdmin", "Admin", "Manager", "Supervisor", "Employee", "Auditor", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        if (!string.IsNullOrWhiteSpace(adminBootstrap.SuperAdminPassword))
        {
            var existingSuperAdmin = await userManager.FindByEmailAsync(adminBootstrap.SuperAdminEmail);
            if (existingSuperAdmin is null)
            {
                var superAdmin = new ApplicationUser
                {
                    UserName = adminBootstrap.SuperAdminEmail,
                    Email = adminBootstrap.SuperAdminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    ForcePasswordChange = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(superAdmin, adminBootstrap.SuperAdminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }
        }
        else
        {
            logger.LogWarning("Super admin password is empty. Skipping bootstrap user creation.");
        }

        if (!await db.ArchiveSettings.AnyAsync())
        {
            db.ArchiveSettings.Add(new ArchiveSettings
            {
                StoragePath = "/app/archiwum",
                AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".xlsx", ".docx"],
                MaxFileSizeMb = 50,
                ModifiedBy = "system"
            });
        }

        if (!await db.ArchiveCategories.AnyAsync())
        {
            db.ArchiveCategories.AddRange(
                new ArchiveCategory { Name = "BHP", Code = "BHP", IsActive = true },
                new ArchiveCategory { Name = "Produkcja", Code = "PROD", IsActive = true },
                new ArchiveCategory { Name = "HR", Code = "HR", IsActive = true },
                new ArchiveCategory { Name = "Jakosc", Code = "QA", IsActive = true },
                new ArchiveCategory { Name = "Finanse", Code = "FIN", IsActive = true }
            );
        }

        if (!await db.ModulePermissions.AnyAsync())
        {
            var roleLookup = await roleManager.Roles.ToDictionaryAsync(x => x.Name!, x => x.Id);
            db.ModulePermissions.AddRange(
                CreateRolePermission(roleLookup, "Employee", ModuleType.Dashboard, canView: true),
                CreateRolePermission(roleLookup, "Employee", ModuleType.Forms, canView: true, canCreate: true, canEdit: true),
                CreateRolePermission(roleLookup, "Manager", ModuleType.Forms, canView: true, canApprove: true, canExport: true),
                CreateRolePermission(roleLookup, "Manager", ModuleType.Reports, canView: true, canExport: true),
                CreateRolePermission(roleLookup, "Admin", ModuleType.Archive, canView: true, canCreate: true, canEdit: true, canDelete: true, canExport: true),
                CreateRolePermission(roleLookup, "Admin", ModuleType.UserManagement, canView: true, canCreate: true, canEdit: true, canDelete: true),
                CreateRolePermission(roleLookup, "Auditor", ModuleType.Audit, canView: true, canExport: true),
                CreateRolePermission(roleLookup, "Viewer", ModuleType.Dashboard, canView: true)
            );
        }

        await db.SaveChangesAsync();
    }

    private static ModulePermission CreateRolePermission(IReadOnlyDictionary<string, string> roleLookup, string roleName, ModuleType module, bool canView = false, bool canCreate = false, bool canEdit = false, bool canDelete = false, bool canApprove = false, bool canExport = false)
        => new()
        {
            RoleId = roleLookup[roleName],
            Module = module,
            CanView = canView,
            CanCreate = canCreate,
            CanEdit = canEdit,
            CanDelete = canDelete,
            CanApprove = canApprove,
            CanExport = canExport
        };
}
