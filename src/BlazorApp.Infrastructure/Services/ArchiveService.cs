using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class ArchiveService : IArchiveService
{
    private readonly AppDbContext _dbContext;
    private readonly IArchiveFileService _archiveFileService;

    public ArchiveService(AppDbContext dbContext, IArchiveFileService archiveFileService)
    {
        _dbContext = dbContext;
        _archiveFileService = archiveFileService;
    }

    public async Task<IReadOnlyList<ArchiveDocumentSummaryDto>> GetRecentDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ArchiveDocuments
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt)
            .Take(12)
            .Select(x => new ArchiveDocumentSummaryDto
            {
                Id = x.Id,
                Title = x.Title,
                CategoryName = x.Category.Name,
                FileExtension = x.FileExtension,
                FileSizeBytes = x.FileSizeBytes,
                DocumentDateUtc = x.DocumentDate,
                DocumentNumber = x.DocumentNumber,
                Tags = x.Tags
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> TestStoragePathAsync(string? storagePath = null, CancellationToken cancellationToken = default)
        => _archiveFileService.TestAccessAsync(storagePath, cancellationToken);
}
