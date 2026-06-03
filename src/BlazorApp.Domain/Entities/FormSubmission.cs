using System.Text.Json;
using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class FormSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int FormDefinitionId { get; set; }
    public string SubmittedByUserId { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public bool IsLate { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? RejectionCategoryId { get; set; }
    public Dictionary<string, JsonElement> FieldValues { get; set; } = new();

    public FormDefinition FormDefinition { get; set; } = null!;
    public ApplicationUser SubmittedBy { get; set; } = null!;
    public ApplicationUser? ReviewedBy { get; set; }
    public RejectionCategory? RejectionCategory { get; set; }
    public ICollection<FormAttachment> Attachments { get; set; } = [];
    public ICollection<FormComment> Comments { get; set; } = [];
    public ICollection<FormSubmissionVersion> Versions { get; set; } = [];
}
