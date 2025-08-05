using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using QuickChart.API.Domain.Dto;
using Microsoft.IdentityModel.Tokens;

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
        public async Task<IActionResult> CreateGroup([FromBody] GroupCreateDto groupCreateDto)
        {
            if (string.IsNullOrEmpty(groupCreateDto.name))
                return BadRequest("Group name cannot be empty");

            var group = new ChatGroup 
            { 
                Name = groupCreateDto.name,
                Members = new List<GroupMember>(),
            };
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            group.CreatedBy = userId;
            group.Members.Add(new GroupMember { UserId = userId, GroupId = group.Id });


            if (groupCreateDto.memberIds != null || groupCreateDto.memberIds!.Any())
            {
                foreach (var memberId in groupCreateDto.memberIds!)
                {
                    if (string.IsNullOrEmpty(memberId))
                        continue;

                    var gMember = new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = memberId
                    };

                    //var existingUser = await _context.Users.FindAsync(memberId);
                    //if (existingUser == null)
                    //    return BadRequest($"User with ID {memberId} does not exist");

                    group.Members.Add(gMember);
                }
            }
            

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

            var groups = await _context.ChatGroups
                .AsNoTracking()
                .Where(g => g.Members.Any(m => m.UserId == userId))               // only groups that include current user
                .Include(g => g.Members)                                        
                    .ThenInclude(m => m.User)                                         
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    Members = g.Members.Select(m => new
                    {
                        m.UserId,
                        Email = m.User != null ? m.User.Email : null,
                        UserName = m.User != null ? m.User.UserName : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(groups);
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
