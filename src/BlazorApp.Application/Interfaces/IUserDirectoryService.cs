using BlazorApp.Application.DTOs;

namespace BlazorApp.Application.Interfaces;

public interface IUserDirectoryService
{
    Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
