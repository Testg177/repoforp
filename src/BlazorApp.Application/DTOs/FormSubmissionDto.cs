using BlazorApp.Domain.Enums;

namespace BlazorApp.Application.DTOs;

public record FormSubmissionDto(
    Guid Id,
    int FormDefinitionId,
    string FormTitle,
    string SubmittedByUserId,
    string SubmittedByName,
    DateTime SubmittedAt,
    DateTime DueDate,
    bool IsLate,
    SubmissionStatus Status,
    string? ReviewedByName,
    DateTime? ReviewedAt,
    string? RejectionReason,
    int AttachmentsCount,
    int CommentsCount
);

public record CreateFormSubmissionRequest(
    int FormDefinitionId,
    DateTime DueDate,
    Dictionary<string, object> FieldValues
);

public record ReviewFormSubmissionRequest(
    Guid SubmissionId,
    bool IsApproved,
    string? RejectionReason,
    int? RejectionCategoryId
);
