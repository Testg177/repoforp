namespace BlazorApp.Application.DTOs;

public record ArchiveDocumentDto(
    Guid Id,
    string Title,
    string? Description,
    int CategoryId,
    string CategoryName,
    string FileName,
    string FileExtension,
    long FileSizeBytes,
    DateTime DocumentDate,
    string? DocumentNumber,
    string[] Tags,
    string UploadedByUserId,
    string UploadedByName,
    DateTime UploadedAt
);

public record ArchiveSearchRequest(
    string? Query,
    int? CategoryId,
    string[]? Tags,
    DateTime? DateFrom,
    DateTime? DateTo,
    int Page = 1,
    int PageSize = 20
);
