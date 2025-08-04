using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace QuickChart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-group")]
        public async Task<IActionResult> CreateGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return BadRequest("Group name cannot be empty");

            var group = new ChatGroup { Name = groupName };
            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPost("add-member")]
        public async Task<IActionResult> AddMember(string groupId, string userId)
        {
            if (await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId))
                return BadRequest("User already in group");

            var member = new GroupMember { GroupId = groupId, UserId = userId };
            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();
            return Ok(member);
        }

        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var groupWithMembersRaw = await (from chatGroup in _context.ChatGroups where chatGroup.CreatedBy == userId
                                             join groupMember in _context.GroupMembers.Include(x => x.User)
                                                 on chatGroup.Id equals groupMember.GroupId into groupMembers
                                             select new
                                             {
                                                 chatGroup.Id,
                                                 chatGroup.Name,
                                                 groupMembers
                                             })
                                .AsNoTracking()
                                .ToListAsync();

            // Now project in memory
            var groupWithMembers = groupWithMembersRaw.Select(group => new
            {
                group.Id,
                group.Name,
                Members = group.groupMembers.Select(gm => new
                {
                    gm.UserId,
                    gm.User.Email,
                    gm.User.UserName
                }).ToList()
            }).ToList();


            return Ok(groupWithMembers);
        }
        
        [HttpGet("private-history/{receiverId}")]
        public async Task<IActionResult> GetPrivateChatHistory(string receiverId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            if (string.IsNullOrEmpty(receiverId))
                return Unauthorized("ReceiverId is Required.");


            var messages = await _context.Messages.AsNoTracking()
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == receiverId) ||
                    (m.SenderId == receiverId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Content,
                    m.SentAt,
                    m.SenderId,
                    m.ReceiverId
                })
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("group-history/{groupId}")]
        public async Task<IActionResult> GetGroupChatHistory(string groupId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            if (string.IsNullOrEmpty(groupId))
                return Unauthorized("GroupId is Required.");

            var isMember = await _context.GroupMembers.AnyAsync(g => g.GroupId == groupId && g.UserId == userId);
            if (!isMember)
                return Ok("You are not a member of this group");

            var messages = await _context.Messages.Include(x=> x.Sender).AsNoTracking()
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    m.Content,
                    m.SentAt,
                    m.SenderId,
                    m.Sender.UserName,
                    m.GroupId
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}
