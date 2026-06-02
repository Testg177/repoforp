namespace BlazorApp.Application.DTOs.Configuration;

public sealed class ArchiveOptions
{
    public const string SectionName = "Archive";

    public string DefaultStoragePath { get; init; } = "/app/archiwum";
    public string[] AllowedExtensions { get; init; } = [".pdf", ".jpg", ".jpeg", ".png", ".tiff"];
    public long MaxFileSizeMb { get; init; } = 50;
    public bool EnableFullTextSearch { get; init; }
    public string? IndexPath { get; init; }
}
