using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlazorApp.Infrastructure.Services;

public class PdfService : IPdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateFormSubmissionPdf(FormSubmission submission)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Formularz: {submission.FormDefinition?.Title ?? submission.FormDefinitionId.ToString()}")
                    .SemiBold().FontSize(16);

                page.Content().Column(col =>
                {
                    col.Item().Text($"Status: {submission.Status}");
                    col.Item().Text($"Złożono: {submission.SubmittedAt:dd.MM.yyyy HH:mm}");
                    col.Item().Text($"Termin: {submission.DueDate:dd.MM.yyyy}");
                    col.Item().PaddingTop(10).Text("Wartości pól:").SemiBold();

                    foreach (var kv in submission.FieldValues)
                    {
                        col.Item().Text($"{kv.Key}: {kv.Value}");
                    }
                });

                page.Footer().AlignCenter()
                    .Text(txt =>
                    {
                        txt.Span("Strona ");
                        txt.CurrentPageNumber();
                        txt.Span(" z ");
                        txt.TotalPages();
                    });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateReportPdf(string title, IEnumerable<FormSubmission> submissions)
    {
        var list = submissions.ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Text(title).SemiBold().FontSize(16);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("ID").SemiBold();
                        header.Cell().Text("Status").SemiBold();
                        header.Cell().Text("Złożono").SemiBold();
                        header.Cell().Text("Termin").SemiBold();
                    });

                    foreach (var s in list)
                    {
                        table.Cell().Text(s.Id.ToString()[..8]);
                        table.Cell().Text(s.Status.ToString());
                        table.Cell().Text(s.SubmittedAt.ToString("dd.MM.yyyy"));
                        table.Cell().Text(s.DueDate.ToString("dd.MM.yyyy"));
                    }
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Strona ");
                    txt.CurrentPageNumber();
                    txt.Span(" z ");
                    txt.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
