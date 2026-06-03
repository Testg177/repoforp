using System.Text.Json;

namespace BlazorApp.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public Dictionary<string, JsonElement>? OldValues { get; set; }
    public Dictionary<string, JsonElement>? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
