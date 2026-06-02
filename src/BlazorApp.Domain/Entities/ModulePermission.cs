using Microsoft.AspNetCore.Identity;
using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class ModulePermission
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? RoleId { get; set; }
    public ModuleType Module { get; set; }
    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanExport { get; set; }

    public ApplicationUser? User { get; set; }
    public IdentityRole? Role { get; set; }
}
