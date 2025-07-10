namespace QuickChart.API.Hub
{
    public class MessageHub: Microsoft.AspNetCore.SignalR.Hub
    {
        public MessageHub()
        {
                
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
