using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RealEstateApp.Presentation.WebApp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNotificationToUser(string userId, string title, string message, string type, string redirectUrl)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
            {
                title,
                message,
                type,
                redirectUrl,
                createdAtFormatted = System.DateTime.Now.ToString("hh:mm tt | dd/MM")
            });
        }
    }
}
