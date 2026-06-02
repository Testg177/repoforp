namespace BlazorApp.Domain.Entities;

public class FormComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormSubmissionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FormSubmission FormSubmission { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
