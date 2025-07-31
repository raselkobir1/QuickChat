using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickChart.API.Helper;

namespace QuickChart.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = RolebasedPolicy.AdminOnly)] // Ensure only Admins can access this controller
    public class AdminController : ControllerBase
    {
        public AdminController() { }

        [HttpGet("dashboard")]
        public IActionResult GetDashboard()
        {
            // This is a placeholder for the actual dashboard data.
            var dashboardData = new
            {
                TotalUsers = 100,
                ActiveUsers = 75,
                TotalChats = 200,
                GroupsCreated = 50
            };

            return Ok(dashboardData);
        }
    }
}
