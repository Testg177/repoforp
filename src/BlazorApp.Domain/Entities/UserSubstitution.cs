namespace BlazorApp.Domain.Entities;

public class UserSubstitution
{
    public int Id { get; set; }
    public string PrincipalUserId { get; set; } = string.Empty;
    public string SubstituteUserId { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public string Scope { get; set; } = "All";

    public ApplicationUser Principal { get; set; } = null!;
    public ApplicationUser Substitute { get; set; } = null!;
}
