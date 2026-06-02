namespace BlazorApp.Application.DTOs;

public sealed class SystemTickerDto
{
    public bool SystemOk { get; init; } = true;
    public int PendingApprovals { get; init; }
    public int OverdueForms { get; init; }
    public int EmailQueueCount { get; init; }
    public DateTime SnapshotAtUtc { get; init; } = DateTime.UtcNow;
}
