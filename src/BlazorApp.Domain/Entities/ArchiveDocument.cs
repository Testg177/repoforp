namespace BlazorApp.Domain.Entities;

public class ArchiveDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime DocumentDate { get; set; }
    public string? DocumentNumber { get; set; }
    public string[] Tags { get; set; } = [];
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public string? Checksum { get; set; }

    public ArchiveCategory Category { get; set; } = null!;
    public ICollection<ArchiveAccessLog> AccessLogs { get; set; } = [];
}
