using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface IArchiveService
{
    Task<IReadOnlyList<ArchiveDocumentSummaryDto>> GetRecentDocumentsAsync(CancellationToken cancellationToken = default);
    Task<bool> TestStoragePathAsync(string? storagePath = null, CancellationToken cancellationToken = default);
}
