using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly IFormsService _formsService;
    private readonly INotificationService _notificationService;
    private readonly IReportsService _reportsService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public DashboardService(AppDbContext dbContext, IFormsService formsService, INotificationService notificationService, IReportsService reportsService, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _formsService = formsService;
        _notificationService = notificationService;
        _reportsService = reportsService;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<SystemTickerDto> GetSystemTickerAsync(CancellationToken cancellationToken = default)
    {
        var pendingApprovals = await _dbContext.FormSubmissions.CountAsync(x => x.Status == SubmissionStatus.Submitted || x.Status == SubmissionStatus.InReview, cancellationToken);
        var overdue = await _dbContext.FormSubmissions.CountAsync(x => x.IsLate && x.Status != SubmissionStatus.Approved && x.Status != SubmissionStatus.Rejected, cancellationToken);
        var emailQueueCount = await _dbContext.EmailQueue.CountAsync(x => x.Status == "Pending", cancellationToken);

        return new SystemTickerDto
        {
            SystemOk = true,
            PendingApprovals = pendingApprovals,
            OverdueForms = overdue,
            EmailQueueCount = emailQueueCount,
            SnapshotAtUtc = DateTime.UtcNow
        };
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserAccessor.UserId;
        var query = _dbContext.FormSubmissions.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(x => x.SubmittedByUserId == userId);
        }

        var drafts = await query.CountAsync(x => x.Status == SubmissionStatus.Draft, cancellationToken);
        var submitted = await query.CountAsync(x => x.Status == SubmissionStatus.Submitted || x.Status == SubmissionStatus.InReview, cancellationToken);
        var approved = await query.CountAsync(x => x.Status == SubmissionStatus.Approved, cancellationToken);
        var overdue = await query.CountAsync(x => x.IsLate && x.Status != SubmissionStatus.Approved && x.Status != SubmissionStatus.Rejected, cancellationToken);

        return new DashboardOverviewDto
        {
            DraftCount = drafts,
            SubmittedCount = submitted,
            ApprovedCount = approved,
            OverdueCount = overdue,
            MyRecentForms = await _formsService.GetMySubmissionsAsync(cancellationToken),
            Notifications = await _notificationService.GetUnreadAsync(cancellationToken),
            ReportTiles = await _reportsService.GetTilesAsync(cancellationToken)
        };
    }
}
