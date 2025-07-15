using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using System.Security.Claims;
namespace QuickChart.API.Hub
{
    [Authorize]
    public class ChatHub: Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AppDbContext _dbContext;
        private readonly IDictionary<string, UserRoomConnection> _connectedUsers;

        public ChatHub(AppDbContext dbContext, IDictionary<string, UserRoomConnection> connectedUsers)
        {
            _dbContext = dbContext;
            _connectedUsers = connectedUsers ?? throw new ArgumentNullException(nameof(connectedUsers));
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectedUsers.TryGetValue(Context.ConnectionId, out var groupUsers)) 
            {
                await Clients.Group($"group_{groupUsers.GroupId}").SendAsync("ReceiveMessage", "System generated", $"{groupUsers.User} has Left the Group", DateTime.Now);
                _connectedUsers.Remove(Context.ConnectionId);
                await SendConnectedUsers(groupUsers.GroupId!);
            }
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessageToUser(string receiverId, string message)
        {
            var senderId = Context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentException("User unAuthinticate.");
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(message))
                throw new ArgumentException("Receiver ID, and message cannot be null or empty.");

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = message
            };

            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message); // Echo to sender
        }

        public async Task SendMessageToGroup(string groupId, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentException("User unAuthinticate.");
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(message))
                throw new ArgumentException("group ID and message cannot be null or empty.");

            var newMessage = new Message
            {
                SenderId = senderId,
                GroupId = groupId,
                Content = message
            };

            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();

            await Clients.Group($"group_{groupId}").SendAsync("ReceiveGroupMessage", senderId, groupId, message);
        }
        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");

            _connectedUsers[Context.ConnectionId] = new UserRoomConnection { GroupId = groupId, User = "Anonomus user"}; 
            await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", "System generated", $"New user has Joined the Group", DateTime.Now);
            await SendConnectedUsers(groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
        }
        public Task SendConnectedUsers(string groupId)
        {
            var users = _connectedUsers.Where(x => x.Value.GroupId == $"group_{groupId}").Select(x => x.Value.User).ToList();
            return Clients.Group($"group_{groupId}").SendAsync("ReceiveConnectedUsers", users);
        }
    }
}
