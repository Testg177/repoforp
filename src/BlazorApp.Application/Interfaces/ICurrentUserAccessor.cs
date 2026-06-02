namespace BlazorApp.Application.Interfaces;

public interface ICurrentUserAccessor
{
    string? UserId { get; }
    string UserName { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
}
