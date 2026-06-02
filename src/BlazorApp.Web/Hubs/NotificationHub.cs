using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Web.Hubs;

public sealed class NotificationHub : Hub
{
    public async Task JoinUserGroup(string userId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, userId);
}
