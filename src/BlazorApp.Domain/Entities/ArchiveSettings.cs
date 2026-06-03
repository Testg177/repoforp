namespace BlazorApp.Domain.Entities;

public class ArchiveSettings
{
    public int Id { get; set; } = 1;
    public string StoragePath { get; set; } = string.Empty;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".png", ".tiff"];
    public long MaxFileSizeMb { get; set; } = 50;
    public bool EnableFullTextSearch { get; set; } = false;
    public string? IndexPath { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = string.Empty;
}
