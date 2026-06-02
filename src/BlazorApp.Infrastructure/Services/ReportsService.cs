using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class ReportsService : IReportsService
{
    private readonly AppDbContext _dbContext;

    public ReportsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ReportTileDto>> GetTilesAsync(CancellationToken cancellationToken = default)
    {
        var total = await _dbContext.FormSubmissions.CountAsync(cancellationToken);
        var approved = await _dbContext.FormSubmissions.CountAsync(x => x.Status == SubmissionStatus.Approved, cancellationToken);
        var rejected = await _dbContext.FormSubmissions.CountAsync(x => x.Status == SubmissionStatus.Rejected, cancellationToken);
        var archiveCount = await _dbContext.ArchiveDocuments.CountAsync(x => !x.IsDeleted, cancellationToken);

        var approvedRatio = total == 0 ? 0 : approved * 100.0 / total;

        return
        [
            new ReportTileDto { Title = "Terminowosc", Value = approvedRatio.ToString("0.0") + "%", Accent = "green", Description = "Udzial zatwierdzonych" },
            new ReportTileDto { Title = "Odrzucone", Value = rejected.ToString(), Accent = "red", Description = "Formularze odrzucone" },
            new ReportTileDto { Title = "Archiwum", Value = archiveCount.ToString(), Accent = "blue", Description = "Dokumenty aktywne" },
            new ReportTileDto { Title = "Wyslane", Value = total.ToString(), Accent = "amber", Description = "Wszystkie zgloszenia" }
        ];
    }
}
