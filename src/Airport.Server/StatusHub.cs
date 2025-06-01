using Microsoft.AspNetCore.SignalR;

namespace Airport.Hubs
{
    public class StatusHub : Hub
    {
        // update хийх үед бусад client-д push хийх
        public async Task SendSeatUpdate(string seatNumber, bool isBooked)
        {
            await Clients.Others.SendAsync("ReceiveSeatUpdate", seatNumber, isBooked);
        }
    }
}
