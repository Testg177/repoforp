namespace BlazorApp.Application.DTOs.Configuration;

public sealed class SchedulerOptions
{
    public const string SectionName = "Scheduler";

    public int DeadlineReminderHours { get; init; } = 24;
    public int EscalationAfterDays { get; init; } = 3;
    public string DailyDigestTime { get; init; } = "07:00";
}
