using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Entities;
using System.Security.Claims;
namespace QuickChart.API.Hub
{
    [Authorize]
    public class ChatHub: Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
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

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

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

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            await Clients.Group($"group_{groupId}").SendAsync("ReceiveGroupMessage", senderId, groupId, message);
        }
    }
}
