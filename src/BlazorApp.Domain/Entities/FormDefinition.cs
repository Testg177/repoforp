using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class FormDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public bool RequiresApproval { get; set; } = true;
    public int? EscalationAfterDays { get; set; } = 3;
    public string[] AssignedRoles { get; set; } = [];
    public string[] AssignedDepartments { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserId { get; set; }

    public ICollection<FormField> Fields { get; set; } = [];
    public ICollection<FormSchedule> Schedules { get; set; } = [];
    public ICollection<FormSubmission> Submissions { get; set; } = [];
    public ICollection<RejectionCategory> RejectionCategories { get; set; } = [];
}
