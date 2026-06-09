using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp.Domain.Entities;

public class CalendarEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public string[] AssignedUserIds { get; set; } = [];
    public int? RelatedFormDefinitionId { get; set; }
    public bool IsCompleted { get; set; }
    public string EventColor { get; set; } = "blue";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FormDefinition? RelatedFormDefinition { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public ApplicationUser CreatedBy { get; set; } = null!;
}
