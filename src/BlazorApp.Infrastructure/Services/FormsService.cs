using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Enums;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class FormsService : IFormsService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public FormsService(AppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyList<FormSummaryDto>> GetMySubmissionsAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return [];
        }

        return await _dbContext.FormSubmissions
            .AsNoTracking()
            .Include(x => x.FormDefinition)
            .Include(x => x.SubmittedBy)
            .Where(x => x.SubmittedByUserId == _currentUserAccessor.UserId)
            .OrderByDescending(x => x.SubmittedAt)
            .Take(12)
            .Select(x => new FormSummaryDto
            {
                Id = x.Id,
                FormCode = x.FormDefinition.Code,
                FormTitle = x.FormDefinition.Title,
                Status = x.Status,
                DueDateUtc = x.DueDate,
                SubmittedAtUtc = x.SubmittedAt,
                IsLate = x.IsLate,
                SubmittedByName = x.SubmittedBy.FirstName + " " + x.SubmittedBy.LastName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FormSummaryDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FormSubmissions
            .AsNoTracking()
            .Include(x => x.FormDefinition)
            .Include(x => x.SubmittedBy)
            .Where(x => x.Status == SubmissionStatus.Submitted || x.Status == SubmissionStatus.InReview)
            .OrderBy(x => x.DueDate)
            .Take(20)
            .Select(x => new FormSummaryDto
            {
                Id = x.Id,
                FormCode = x.FormDefinition.Code,
                FormTitle = x.FormDefinition.Title,
                Status = x.Status,
                DueDateUtc = x.DueDate,
                SubmittedAtUtc = x.SubmittedAt,
                IsLate = x.IsLate,
                SubmittedByName = x.SubmittedBy.FirstName + " " + x.SubmittedBy.LastName
            })
            .ToListAsync(cancellationToken);
    }
}
