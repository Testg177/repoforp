namespace BlazorApp.Application.Interfaces;

public interface IArchiveFileService
{
    Task<string> SaveAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);
    Task<string> ComputeSha256Async(Stream stream, CancellationToken cancellationToken = default);
    Task<bool> TestAccessAsync(string? storagePath = null, CancellationToken cancellationToken = default);
}
