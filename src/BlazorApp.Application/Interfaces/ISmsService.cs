namespace BlazorApp.Application.Interfaces;

public interface ISmsService
{
    Task SendAsync(string toPhoneNumber, string message, CancellationToken ct = default);
    Task QueueAsync(string toPhoneNumber, string message);
}
