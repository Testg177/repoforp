namespace BlazorApp.Domain.Entities;

public class FormAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid FormSubmissionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;

    public FormSubmission FormSubmission { get; set; } = null!;
}
