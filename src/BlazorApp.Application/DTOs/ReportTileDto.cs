namespace BlazorApp.Application.DTOs;

public sealed class ReportTileDto
{
    public string Title { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Accent { get; init; } = "green";
    public string Description { get; init; } = string.Empty;
}
