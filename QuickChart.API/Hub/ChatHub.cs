using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using System.Security.Claims;
namespace QuickChart.API.Hub
{
    [Authorize]
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AppDbContext _dbContext;
        private static readonly Dictionary<string, HashSet<string>> _activeGroupMembers = new Dictionary<string, HashSet<string>>();

        public ChatHub(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            lock (_activeGroupMembers)
            {
                foreach (var group in _activeGroupMembers.Keys.ToList())
                {
                    _activeGroupMembers[group].RemoveWhere(u => u == Context.UserIdentifier);

                    if (_activeGroupMembers[group].Count > 0)
                    {
                        //SendConnectedUsers(group);
                    }
                }
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

            var newMessage = GetMessage(senderId, message, receiverId, null, null);

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

            if (!_dbContext.GroupMembers.Any(x => x.GroupId == groupId && x.UserId == senderId))
            {
                var msg = "Currently you are not in this group member.";
                var sysMessageObj = GetMessage(senderId!, msg, null, groupId, userName: "System generated");
                await Clients.Caller.SendAsync("ReceiveMessage", sysMessageObj); // Echo to sender
            }
            var newMessage = GetMessage(senderId, message, null, groupId, userName);
            _dbContext.Messages.Add(newMessage);
            await _dbContext.SaveChangesAsync();

            await Clients.Group($"group_{groupId}").SendAsync("ReceiveMessage", newMessage);
        }
        public async Task JoinGroup(string groupId)
        {
            var newJoinUserId = Context.UserIdentifier;
            var userName = Context.User?.FindFirst(ClaimTypes.Surname)?.Value;

            if (_activeGroupMembers.ContainsKey(groupId))
            {
                if (_activeGroupMembers[groupId].Contains(newJoinUserId!))
                {
                    return;
                }
            }

            if (!_dbContext.GroupMembers.Any(x => x.GroupId == groupId && x.UserId == newJoinUserId))
            {
                var msg = "Currently you are not in this group member.";
                var sysMessageObj = GetMessage(newJoinUserId!, msg, null, groupId, userName: "System generated");
                await Clients.Caller.SendAsync("ReceiveMessage", sysMessageObj); // Echo to sender
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
            lock (_activeGroupMembers)
            {
                if (!_activeGroupMembers.ContainsKey(groupId))
                    _activeGroupMembers[groupId] = new HashSet<string>();

                _activeGroupMembers[groupId].Add(newJoinUserId!);
            }

            var message = $"{userName} has Joined the Group";
            var newMessage = GetMessage(newJoinUserId!, message, null, groupId, userName: "System generated");
            await Clients.OthersInGroup($"group_{groupId}").SendAsync("ReceiveMessage", newMessage);
            //await SendConnectedUsers(groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            var leavedUserId = Context.UserIdentifier;
            var userName = Context.User?.FindFirst(ClaimTypes.Surname)?.Value;

            if (!_activeGroupMembers.ContainsKey(groupId) || !_activeGroupMembers[groupId].Contains(leavedUserId!))
                return;

            _activeGroupMembers[groupId].RemoveWhere(u => u == Context.UserIdentifier);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");

            var message = $"{userName} has leave the chat";
            var newMessage = GetMessage(leavedUserId!, message, null, groupId, userName: "System generated");
            await Clients.OthersInGroup($"group_{groupId}").SendAsync("ReceiveMessage", newMessage);
            await SendConnectedUsers(groupId);
        }
        public Task SendConnectedUsers(string groupId)
        {
            if (!_activeGroupMembers.ContainsKey(groupId))
            {
                return Clients.Group($"group_{groupId}").SendAsync("ReceiveConnectedUsers", groupId, new HashSet<string>());
            }
            var usersIds = _activeGroupMembers[groupId];
            return Clients.Group($"group_{groupId}").SendAsync("ReceiveConnectedUsers", groupId, usersIds);
        }

        private Message GetMessage(string senderId, string message, string? receiverId = null, string? groupId = null, string? userName = null)
        {
            var newMessage = new Message
            {
                SenderId = senderId,
                GroupId = groupId,
                Content = message,
                SentAt = DateTime.UtcNow,
                ReceiverId = receiverId,
                UserName = userName
            };
            return newMessage;
        }
    }
}