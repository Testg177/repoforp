namespace BlazorApp.Application.DTOs;

public record UserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? Department,
    string? Position,
    bool IsActive,
    bool ForcePasswordChange,
    DateTime? LastLoginAt,
    IList<string> Roles
);

public record CreateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? Department,
    string? Position,
    string Password,
    IList<string> Roles
);

public record UpdateUserRequest(
    string FirstName,
    string LastName,
    string? Department,
    string? Position,
    bool IsActive,
    bool ReceiveEmailNotifications,
    bool ReceiveSmsNotifications,
    IList<string> Roles
);
