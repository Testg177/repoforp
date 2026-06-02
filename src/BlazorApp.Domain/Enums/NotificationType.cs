namespace BlazorApp.Domain.Enums;

public enum NotificationType
{
    FormDeadlineReminder,
    FormSubmittedForApproval,
    FormApproved,
    FormRejected,
    FormRequiresChanges,
    ArchiveDocumentUploaded,
    SystemAlert,
    TaskAssigned
}
