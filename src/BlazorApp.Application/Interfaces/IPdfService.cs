using BlazorApp.Domain.Entities;

namespace BlazorApp.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateFormSubmissionPdf(FormSubmission submission);
    byte[] GenerateReportPdf(string title, IEnumerable<FormSubmission> submissions);
}
