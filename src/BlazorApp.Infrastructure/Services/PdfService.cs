using BlazorApp.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlazorApp.Infrastructure.Services;

public sealed class PdfService : IPdfService
{
    public byte[] GenerateSimpleReport(string title, IEnumerable<(string Label, string Value)> rows)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);
                page.Content().Column(column =>
                {
                    column.Item().Text(title).FontSize(20).Bold();
                    column.Item().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().PaddingVertical(4).Text(row.Label);
                            table.Cell().PaddingVertical(4).Text(row.Value);
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
