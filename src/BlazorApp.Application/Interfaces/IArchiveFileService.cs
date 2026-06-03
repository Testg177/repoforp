namespace BlazorApp.Application.Interfaces;

public interface IArchiveFileService
{
    Task<(string relativePath, string storedFileName, string checksum)> SaveFileAsync(
        Stream fileStream, string originalFileName, string category, CancellationToken ct = default);
    Task DeleteFileAsync(string relativePath);
    string GetFullPath(string relativePath);
    bool IsAllowedExtension(string fileName);
    bool IsWithinSizeLimit(long fileSizeBytes);
}
