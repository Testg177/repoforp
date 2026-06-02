using System.Text.Json;

namespace BlazorApp.Domain.Entities;

public class FormSubmissionVersion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormSubmissionId { get; set; }
    public int VersionNumber { get; set; }
    public Dictionary<string, JsonElement> FieldValues { get; set; } = new();
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    public string SavedByUserId { get; set; } = string.Empty;
    public string? ChangeComment { get; set; }

    public FormSubmission FormSubmission { get; set; } = null!;
}
