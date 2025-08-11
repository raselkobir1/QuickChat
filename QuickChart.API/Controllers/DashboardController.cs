using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickChart.API.Domain;

namespace QuickChart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var totalUsers = _context.Users.Count();
            var totalMessages = _context.Messages.Count();
            var totalGroups = _context.ChatGroups.Count();

            return Ok(new
            {
                totalUsers,
                totalMessages,
                totalGroups
            });
        }
    }
}
