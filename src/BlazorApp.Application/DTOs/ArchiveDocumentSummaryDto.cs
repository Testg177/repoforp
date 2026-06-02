namespace BlazorApp.Application.DTOs;

public sealed class ArchiveDocumentSummaryDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public string FileExtension { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTime DocumentDateUtc { get; init; }
    public string? DocumentNumber { get; init; }
    public string[] Tags { get; init; } = [];
}
