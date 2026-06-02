using BlazorApp.Domain.Enums;

namespace BlazorApp.Domain.Entities;

public class FormSchedule
{
    public int Id { get; set; }
    public int FormDefinitionId { get; set; }
    public ScheduleType Type { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public TimeOnly DueTime { get; set; }
    public int ReminderHoursBefore { get; set; } = 24;
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? CronExpression { get; set; }

    public FormDefinition FormDefinition { get; set; } = null!;
}
