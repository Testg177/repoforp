namespace BlazorApp.Application.Interfaces;

public interface ISmsService
{
    Task QueueAsync(string toPhoneNumber, string message, CancellationToken cancellationToken = default);
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
