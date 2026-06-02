namespace BlazorApp.Application.DTOs;

public sealed class DashboardOverviewDto
{
    public int DraftCount { get; init; }
    public int SubmittedCount { get; init; }
    public int ApprovedCount { get; init; }
    public int OverdueCount { get; init; }
    public IReadOnlyList<FormSummaryDto> MyRecentForms { get; init; } = [];
    public IReadOnlyList<NotificationListItemDto> Notifications { get; init; } = [];
    public IReadOnlyList<ReportTileDto> ReportTiles { get; init; } = [];
}
