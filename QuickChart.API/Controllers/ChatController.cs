using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Domain;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using QuickChart.API.Domain.Dto;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.HttpResults;

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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");
            if (string.IsNullOrEmpty(groupCreateDto.name.Trim()))
                return BadRequest("Group name cannot be empty");

            var group = new ChatGroup
            {
                Name = groupCreateDto.name.Trim(),
                Members = new List<GroupMember>(),
            };
            group.CreatedBy = userId;
            group.Members.Add(new GroupMember { UserId = userId, GroupId = group.Id }); // Added Current usser to this group.

            if (groupCreateDto.memberIds != null || groupCreateDto.memberIds!.Any())
            {
                var notExistingUserIds = groupCreateDto.memberIds?.Except(await _context.Users.Select(x => x.Id).ToListAsync()).ToList();
                if (notExistingUserIds!.Any())
                    return BadRequest($"User(s) with ID(s) {string.Join(", ", notExistingUserIds!)} do not exist in database.");

                foreach (var memberId in groupCreateDto.memberIds!)
                {
                    if (string.IsNullOrEmpty(memberId))
                        continue;

                    var gMember = new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = memberId
                    };
                    group.Members.Add(gMember);
                }
            }

            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpPost("add-member")]
        public async Task<IActionResult> AddMember([FromBody] AssignUserToGroupDto assignUser)
        {
            if (string.IsNullOrEmpty(assignUser.GroupId))
                return BadRequest("GroupId and UserId are required");

            if (assignUser.memberIds == null || !assignUser.memberIds.Any())
                return BadRequest("At least one User is required to add to the group");

            if (!await _context.ChatGroups.AnyAsync(x => x.Id == assignUser.GroupId))
                return NotFound($"Group with ID {assignUser.GroupId} does not exist");

            var groupId = assignUser.GroupId;
            var memberIds = assignUser.memberIds;

            var notExistingUserIds = memberIds.Except(await _context.Users.Select(x => x.Id).ToListAsync()).ToList();
            if (notExistingUserIds.Any())
                return BadRequest($"User(s) with ID(s) {string.Join(", ", notExistingUserIds)} do not exist in database.");

            var existsAny = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && memberIds.Contains(gm.UserId));

            if (existsAny)
            {
                var existingUsers = await _context.GroupMembers.Include(x => x.User)
                            .Where(gm => gm.GroupId == groupId && memberIds.Contains(gm.UserId))
                            .Select(gm => gm.User.UserName)
                            .ToListAsync();
                return Ok($"User(s) {string.Join(", ", existingUsers)} are already members of this group.");
            }

            var groupMembers = new List<GroupMember>();
            foreach (var memberId in memberIds)
            {
                var groupMember = new GroupMember
                {
                    GroupId = groupId,
                    UserId = memberId
                };
                groupMembers.Add(groupMember);
            }
            _context.GroupMembers.AddRange(groupMembers);
            await _context.SaveChangesAsync();
            return Ok(groupMembers);
        }

        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not authenticated");

            var groups = await _context.ChatGroups
                .AsNoTracking()
                .Where(g => g.Members.Any(m => m.UserId == userId))  // only get groups that include current user
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
                return BadRequest("ReceiverId is Required.");


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
                return BadRequest("GroupId is Required.");

            var isMember = await _context.GroupMembers.AnyAsync(g => g.GroupId == groupId && g.UserId == userId);
            if (!isMember)
                return Ok("You are not a member of this group");

            var messages = await _context.Messages.Include(x => x.Sender).AsNoTracking()
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
