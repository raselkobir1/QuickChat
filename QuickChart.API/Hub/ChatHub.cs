using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using System.Security.Claims;
using System.Text.RegularExpressions;
namespace QuickChart.API.Hub
{
    [Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
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
            if (_connectedUsers.TryGetValue(Context.ConnectionId, out var groupUser))
            {
                _connectedUsers.Remove(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupUser.GroupId}");
                //await SendConnectedUsers(groupUser.GroupId!);
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
                Content = message,
                SentAt = DateTime.UtcNow,
                GroupId = null
            };

            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceiveMessage", newMessage);
            await Clients.Caller.SendAsync("ReceiveMessage", newMessage); // Echo to sender
        }

        public async Task SendMessageToGroup(string groupId, string message)
        {
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = Context.User?.FindFirst(ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(senderId))
                throw new ArgumentException("User unAuthinticate.");
            if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(message))
                throw new ArgumentException("group ID and message cannot be null or empty.");

            var newMessage = new Message
            {
                SenderId = senderId,
                GroupId = groupId,
                Content = message,
                SentAt = DateTime.UtcNow,
                ReceiverId = null 
            };

            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();
            newMessage.UserName = userName;
            await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", newMessage);
        }
        public async Task JoinGroup(string groupId)
        {
            if (_connectedUsers.ContainsKey(Context.ConnectionId) && _connectedUsers[Context.ConnectionId].GroupId == groupId)
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
            var userName = Context.User?.FindFirst(ClaimTypes.Surname)?.Value;
            var newJoinUserId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            _connectedUsers[Context.ConnectionId] = new UserRoomConnection { GroupId = groupId, User = userName };

            var newMessage = new Message
            {
                SenderId = newJoinUserId,
                GroupId = groupId,
                Content = $"{userName} has Joined the Group",
                UserName = "System user",
                SentAt = DateTime.UtcNow,
                ReceiverId = null 
            };
            await Clients.OthersInGroup($"group_{groupId}").SendAsync("ReceiveMessage", newMessage);
            //await SendConnectedUsers(groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            _connectedUsers.Remove(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
            await SendConnectedUsers(groupId);
        }
        public Task SendConnectedUsers(string groupId)
        {
            var users = _connectedUsers.Where(x => x.Value.GroupId == $"group_{groupId}").Select(x => x.Value.User).ToList();
            return Clients.Group($"group_{groupId}").SendAsync("ReceiveConnectedUsers", users);
        }
    }
}