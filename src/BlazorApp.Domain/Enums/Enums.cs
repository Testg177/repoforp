namespace BlazorApp.Domain.Enums;

public enum ModuleType
{
    Dashboard, Forms, Archive, Reports,
    UserManagement, SystemSettings, Notifications, Audit
}

public enum SubmissionStatus
{
    Draft, Submitted, InReview, Approved, Rejected, RequiresChanges
}

public enum ScheduleType { Daily, Weekly, Monthly, Custom, OneTime }

public enum NotificationType
{
    FormDeadlineReminder, FormSubmittedForApproval, FormApproved,
    FormRejected, FormRequiresChanges, ArchiveDocumentUploaded,
    SystemAlert, TaskAssigned
}

public enum FieldType
{
    Text, TextArea, Number, Decimal,
    Date, DateTime, Time,
    Select, MultiSelect, Radio, Checkbox,
    FileUpload, Signature, Table, Section, CalculatedField
}
