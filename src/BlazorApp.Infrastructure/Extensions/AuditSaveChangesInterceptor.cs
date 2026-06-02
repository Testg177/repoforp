using System.Text.Json;
using BlazorApp.Application.Interfaces;
using BlazorApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BlazorApp.Infrastructure.Extensions;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AuditSaveChangesInterceptor(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AppendAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AppendAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is not AuditLog && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var auditLogs = new List<AuditLog>(entries.Count);
        foreach (var entry in entries)
        {
            auditLogs.Add(new AuditLog
            {
                UserId = _currentUserAccessor.UserId,
                UserName = _currentUserAccessor.UserName,
                Action = entry.State.ToString(),
                EntityType = entry.Metadata.ClrType.Name,
                EntityId = GetEntityId(entry),
                OldValues = entry.State == EntityState.Added ? null : GetValues(entry.OriginalValues),
                NewValues = entry.State == EntityState.Deleted ? null : GetValues(entry.CurrentValues),
                IpAddress = _currentUserAccessor.IpAddress,
                Timestamp = DateTime.UtcNow,
                IsSuccess = true
            });
        }

        context.Set<AuditLog>().AddRange(auditLogs);
    }

    private static string? GetEntityId(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        var property = primaryKey?.Properties.FirstOrDefault();
        return property is null ? null : entry.Property(property.Name).CurrentValue?.ToString();
    }

    private static Dictionary<string, JsonElement>? GetValues(PropertyValues propertyValues)
    {
        var result = new Dictionary<string, JsonElement>();
        foreach (var property in propertyValues.Properties)
        {
            var value = propertyValues[property];
            if (value is null)
            {
                continue;
            }

            result[property.Name] = JsonSerializer.SerializeToElement(value, value.GetType());
        }

        return result.Count == 0 ? null : result;
    }
}
