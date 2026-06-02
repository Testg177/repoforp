using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Position { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ForcePasswordChange { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumberForSms { get; set; }
    public bool ReceiveEmailNotifications { get; set; } = true;
    public bool ReceiveSmsNotifications { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Nawigacje
    public ICollection<ModulePermission> ModulePermissions { get; set; } = [];
}
