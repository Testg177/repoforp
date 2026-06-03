using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public static async Task SendToUserAsync(IHubContext<NotificationHub> hub, string userId, string method, object payload)
    {
        await hub.Clients.Group($"user_{userId}").SendAsync(method, payload);
    }
}
