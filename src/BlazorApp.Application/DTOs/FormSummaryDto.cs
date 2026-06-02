using BlazorApp.Domain.Enums;

namespace BlazorApp.Application.DTOs;

public sealed class FormSummaryDto
{
    public Guid Id { get; init; }
    public string FormCode { get; init; } = string.Empty;
    public string FormTitle { get; init; } = string.Empty;
    public SubmissionStatus Status { get; init; }
    public DateTime DueDateUtc { get; init; }
    public DateTime SubmittedAtUtc { get; init; }
    public bool IsLate { get; init; }
    public string SubmittedByName { get; init; } = string.Empty;
}
