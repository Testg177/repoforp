using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface IReportsService
{
    Task<IReadOnlyList<ReportTileDto>> GetTilesAsync(CancellationToken cancellationToken = default);
}
