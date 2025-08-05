using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Helper.Enums;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickChart.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _configuration = configuration;
        _roleManager = roleManager;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.UserName) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Username, email, and password are required");

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest("Email already exists");

        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        var role = dto.UserType switch
        {
            UserTypes.User => Roles.User.ToString(),
            UserTypes.Admin => Roles.Admin.ToString(),
            UserTypes.SuperAdmin => Roles.SuperAdmin.ToString(),
            _ => null
        };

        if (!await _roleManager.RoleExistsAsync(role!))
        {
            await _roleManager.CreateAsync(new IdentityRole(role!));
        }

        var roleAssignResult = await _userManager.AddToRoleAsync(user, role!);
        if (!roleAssignResult.Succeeded)
            return BadRequest(roleAssignResult.Errors);

        return Ok(new { UserName = user.UserName, Role = role, Message = "User registered successfully" });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid username or password");

        var token = await GenerateJwtToken(user);
        return Ok(new { token, user.Email, user.UserName, user.Id });
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("userName", user.UserName!),

        };
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiresInMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found");

        return Ok(new { user.UserName, user.Email, user.Id });
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var users = _userManager.Users.Where(x=> x.Id != userId).ToList();
        if (users == null || !users.Any())
            return NotFound("No users found");
        return Ok(users.Select(u => new { u.Id, u.UserName, u.Email }));
    }
}
