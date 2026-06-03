using System.Security.Claims;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public class ModulePermissionClaimsTransformer(IDbContextFactory<AppDbContext> factory) : IClaimsTransformation
{
    private const string LoadedClaim = "perm_loaded";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!principal.Identity?.IsAuthenticated ?? true)
            return principal;
        if (principal.HasClaim(c => c.Type == LoadedClaim))
            return principal;

        var identity = (ClaimsIdentity)principal.Identity!;
        var userId   = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return principal;

        await using var db = await factory.CreateDbContextAsync();

        // Collect roleIds for this user
        var roleIds = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync();

        // Load role-level permissions
        var rolePerms = await db.ModulePermissions
            .Where(p => p.RoleId != null && roleIds.Contains(p.RoleId!))
            .ToListAsync();

        // Load user-level permissions (override)
        var userPerms = await db.ModulePermissions
            .Where(p => p.UserId == userId)
            .ToListAsync();

        // Merge: user-level overrides role-level
        var effective = rolePerms
            .GroupBy(p => p.Module)
            .ToDictionary(g => g.Key, g => g.Aggregate((a, b) => new BlazorApp.Domain.Entities.ModulePermission
            {
                Module     = a.Module,
                CanView    = a.CanView    || b.CanView,
                CanCreate  = a.CanCreate  || b.CanCreate,
                CanEdit    = a.CanEdit    || b.CanEdit,
                CanDelete  = a.CanDelete  || b.CanDelete,
                CanApprove = a.CanApprove || b.CanApprove,
                CanExport  = a.CanExport  || b.CanExport,
            }));

        foreach (var up in userPerms)
            effective[up.Module] = up;

        // Add claims
        foreach (var (module, perm) in effective)
        {
            var prefix = $"Module.{module}";
            if (perm.CanView)    identity.AddClaim(new Claim($"{prefix}.View",    "true"));
            if (perm.CanCreate)  identity.AddClaim(new Claim($"{prefix}.Create",  "true"));
            if (perm.CanEdit)    identity.AddClaim(new Claim($"{prefix}.Edit",    "true"));
            if (perm.CanDelete)  identity.AddClaim(new Claim($"{prefix}.Delete",  "true"));
            if (perm.CanApprove) identity.AddClaim(new Claim($"{prefix}.Approve", "true"));
            if (perm.CanExport)  identity.AddClaim(new Claim($"{prefix}.Export",  "true"));
        }

        identity.AddClaim(new Claim(LoadedClaim, "true"));
        return principal;
    }
}
