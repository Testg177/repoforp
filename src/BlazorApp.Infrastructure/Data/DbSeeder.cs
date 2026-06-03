using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = ["SuperAdmin", "Admin", "Manager", "Supervisor",
                          "Employee", "Auditor", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        const string superAdminEmail = "superadmin@firma.pl";
        if (await userManager.FindByEmailAsync(superAdminEmail) is null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FirstName = "Super",
                LastName = "Admin",
                IsActive = true,
                ForcePasswordChange = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(superAdmin, "TymczasoweHaslo123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }

        if (!await db.ArchiveSettings.AnyAsync())
        {
            db.ArchiveSettings.Add(new ArchiveSettings
            {
                StoragePath = "/app/archiwum",
                AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".tiff"],
                MaxFileSizeMb = 50
            });
        }

        if (!await db.ArchiveCategories.AnyAsync())
        {
            db.ArchiveCategories.AddRange(
                new ArchiveCategory { Name = "BHP", Code = "BHP", IsActive = true },
                new ArchiveCategory { Name = "Produkcja", Code = "PROD", IsActive = true },
                new ArchiveCategory { Name = "HR", Code = "HR", IsActive = true },
                new ArchiveCategory { Name = "Jakość", Code = "QA", IsActive = true },
                new ArchiveCategory { Name = "Finanse", Code = "FIN", IsActive = true }
            );
        }

        await db.SaveChangesAsync();
    }
}
