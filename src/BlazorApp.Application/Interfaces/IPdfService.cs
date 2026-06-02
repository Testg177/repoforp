namespace BlazorApp.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateSimpleReport(string title, IEnumerable<(string Label, string Value)> rows);
}
