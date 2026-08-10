using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Booking.API.Hubs
{
    [Authorize]
    public class BookingStatusHub : Hub
    {
        public Task JoinOffer(Guid offerId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, $"offer:{offerId}");

        public Task LeaveOffer(Guid offerId) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, $"offer:{offerId}");
    }
}
