using System.Text.Json;

namespace BlazorApp.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, string? entityId = null,
        Dictionary<string, JsonElement>? oldValues = null,
        Dictionary<string, JsonElement>? newValues = null,
        bool isSuccess = true, string? errorMessage = null);
}
