using BlazorApp.Application.DTOs;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Services;

public sealed class UserDirectoryService : IUserDirectoryService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserDirectoryService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        var result = new List<UserListItemDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserListItemDto
            {
                Id = user.Id,
                FullName = (user.FirstName + " " + user.LastName).Trim(),
                Email = user.Email ?? string.Empty,
                Department = user.Department,
                Position = user.Position,
                IsActive = user.IsActive,
                Roles = roles.ToArray()
            });
        }

        return result;
    }
}
