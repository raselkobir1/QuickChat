using Microsoft.AspNetCore.Mvc;

namespace QuickChart.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase 
    {
        public AuthController()
        {
                
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            await Task.Delay(1000);
            return Unauthorized();
        }
    }
}
