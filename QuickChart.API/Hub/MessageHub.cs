using Microsoft.AspNetCore.SignalR;
using QuickChart.API.Domain.Dto;

namespace QuickChart.API.Hub
{
    public class MessageHub: Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly IDictionary<string, UserRoomConnection> _connection;
        public MessageHub(IDictionary<string, UserRoomConnection> connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connection.TryGetValue(Context.ConnectionId, out var userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room!).SendAsync("ReceiveMessage", "System generated", $"{userRoomConnection.User} has Left the Group", DateTime.Now);
                _connection.Remove(Context.ConnectionId);
                await SendConnectedUsers(userRoomConnection.Room!);
            }
            await base.OnDisconnectedAsync(exception);
        }
        public async Task JoinRoom(UserRoomConnection userConnection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room!);
            _connection[Context.ConnectionId] = userConnection;

            await Clients.Group(userConnection.Room!).SendAsync("ReceiveMessage", "System generated", $"{userConnection.User} has Joined the Group", DateTime.Now);
            await SendConnectedUsers(userConnection.Room!);
        }
        public async Task SendMessage(string message)
        {
            if (_connection.TryGetValue(Context.ConnectionId, out var userRoomConnection))
            {
                await Clients.Group(userRoomConnection.Room!).SendAsync("ReceiveMessage", userRoomConnection.User, message, DateTime.Now);
            }
        }
        public Task SendConnectedUsers(string room) 
        {
            var users = _connection.Where(x => x.Value.Room == room).Select(x => x.Value.User).ToList();
            return Clients.Group(room).SendAsync("ReceiveConnectedUsers", users);
        }
    }
}
