using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface IFormsService
{
    Task<IReadOnlyList<FormSummaryDto>> GetMySubmissionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FormSummaryDto>> GetPendingApprovalsAsync(CancellationToken cancellationToken = default);
}
