using System.Security.Claims;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Security;

public sealed class ModulePermissionClaimsTransformation : IClaimsTransformation
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public ModulePermissionClaimsTransformation(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        if (principal.HasClaim(claim => claim.Type.StartsWith("Module.", StringComparison.Ordinal)))
        {
            return principal;
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return principal;
        }

        var identity = principal.Identity as ClaimsIdentity;
        if (identity is null)
        {
            return principal;
        }

        var roleIds = await _dbContext.UserRoles
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync();

        var permissions = await _dbContext.ModulePermissions
            .Where(x => x.UserId == userId || (x.RoleId != null && roleIds.Contains(x.RoleId)))
            .ToListAsync();

        foreach (var permission in permissions)
        {
            AddClaim(identity, permission.Module, "View", permission.CanView);
            AddClaim(identity, permission.Module, "Create", permission.CanCreate);
            AddClaim(identity, permission.Module, "Edit", permission.CanEdit);
            AddClaim(identity, permission.Module, "Delete", permission.CanDelete);
            AddClaim(identity, permission.Module, "Approve", permission.CanApprove);
            AddClaim(identity, permission.Module, "Export", permission.CanExport);
        }

        return principal;
    }

    private static void AddClaim(ClaimsIdentity identity, Domain.Enums.ModuleType module, string action, bool value)
    {
        identity.AddClaim(new Claim($"Module.{module}.{action}", value.ToString().ToLowerInvariant()));
    }
}
