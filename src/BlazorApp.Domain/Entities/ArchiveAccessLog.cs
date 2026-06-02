namespace BlazorApp.Domain.Entities;

public class ArchiveAccessLog
{
    public long Id { get; set; }
    public Guid ArchiveDocumentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }

    public ArchiveDocument ArchiveDocument { get; set; } = null!;
}
