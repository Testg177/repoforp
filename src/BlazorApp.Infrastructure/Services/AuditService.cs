using System.Text.Json;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using BlazorApp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace BlazorApp.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string action, string entityType, string? entityId = null,
        Dictionary<string, JsonElement>? oldValues = null,
        Dictionary<string, JsonElement>? newValues = null,
        bool isSuccess = true, string? errorMessage = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var userId = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = httpContext?.User?.Identity?.Name ?? "System";
        var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ip,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        });

        await _db.SaveChangesAsync();
    }
}
