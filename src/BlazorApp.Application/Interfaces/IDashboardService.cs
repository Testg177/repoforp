using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface IDashboardService
{
    Task<SystemTickerDto> GetSystemTickerAsync(CancellationToken cancellationToken = default);
    Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default);
}
