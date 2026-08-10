using Microsoft.AspNetCore.SignalR;

namespace Booking.API.Hubs
{
    public class BookingStatusHub : Hub
    {
        // 这里目前不需要额外方法，直接用 Clients.All.SendAsync 即可
    }
}
