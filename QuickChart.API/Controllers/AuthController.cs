using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using QuickChart.API.Helper.Enums;
using QuickChart.API.Services;
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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;
    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrEmpty(dto.FullName) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
            return BadRequest("Full Name, email, and password are required"); 

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest("Email already exists");

        var user = new ApplicationUser { FullName = dto.FullName, UserName = dto.Email, Email = dto.Email };
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

        return Ok(new { UserName = user.FullName, Role = role, Message = "User registered successfully" });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("Invalid email or password");

        var token = await GenerateJwtToken(user);
        return Ok(new { token, user.Email, UserName = user.FullName, user.Id });
    }

    // 1) challenge endpoint -> Angular opens this URL in popup
    [HttpGet("login-with-google")]
    [AllowAnonymous]
    public IActionResult ExternalLoginLogin(string provider, string returnUrl = null)
    {
        var redirectUrl = Url.Action("externallogincallback", "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    } 

    [HttpGet("login-with-facebook")]
    [AllowAnonymous]
    public IActionResult ExternalLoginFacebook(string provider, string returnUrl = null) 
    {
        var redirectUrl = Url.Action("externallogincallback", "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("userName", user.UserName!),
            new Claim(ClaimTypes.Surname, user.FullName ?? user.UserName!),

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
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken) 
    {
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var response = await _tokenService.RefreshTokenAsync(refreshToken, ipAddress);
        return Ok(response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        await _tokenService.RevokeTokenAsync(refreshToken, ipAddress);
        return NoContent();
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

        var profile = new
        {
            user.Id,
            UserName = user.FullName,
            user.Email,
            //Roles = await _userManager.GetRolesAsync(user),
            user.PhoneNumber,
            user.ProfileImageUrl,
            user.CoverImageUrl,
            user.ParmanantAddress,
            user.PresentAddress,
            user.UniversityName,
            user.CollageName,
            user.WorkPlaceName,
            user.DateOfBirth
        };

        return Ok(profile);  
    }

    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User not authenticated");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found");

        user.FullName = dto.FullName ?? user.FullName;
        user.Email = dto.Email ?? user.Email;
        user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;
        user.ProfileImageUrl = dto.ProfileImageUrl ?? user.ProfileImageUrl;
        user.CoverImageUrl = dto.CoverImageUrl ?? user.CoverImageUrl;
        user.ParmanantAddress = dto.ParmanantAddress ?? user.ParmanantAddress;
        user.PresentAddress = dto.PresentAddress ?? user.PresentAddress;
        user.UniversityName = dto.UniversityName ?? user.UniversityName;
        user.CollageName = dto.CollageName ?? user.CollageName;
        user.WorkPlaceName = dto.WorkPlaceName ?? user.WorkPlaceName;
        if (dto.DateOfBirth.HasValue)
            user.DateOfBirth = dto.DateOfBirth.Value;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { Message = "Profile updated successfully" });
    }

    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var users = _userManager.Users.Where(x=> x.Id != userId).ToList();
        if (users == null || !users.Any())
            return NotFound("No users found");
        return Ok(users.Select(u => new { u.Id, UserName = u.FullName, u.Email }));
    }

    [HttpPost("file-upload")]
    public async Task<IActionResult> FileUpload(IFormFile file) 
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { Path = fileUrl }); // return only file name  
    }

    // 2) callback invoked by provider
    [AllowAnonymous]
    [HttpGet("externallogincallback")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            var failed = $"{_configuration["Frontend:Url"]}/auth-failed";
            return Redirect(failed);
        }

        // Try sign-in with external provider
        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        ApplicationUser user = null;

        if (signInResult.Succeeded)
        {
            user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        }
        else
        {
            // Get email from provider
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                var needEmail = $"{_configuration["Frontend:Url"]}/external-email-required";
                return Redirect(needEmail);
            }

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                var createRes = await _userManager.CreateAsync(user);
                if (!createRes.Succeeded)
                    return Redirect($"{_configuration["Frontend:Url"]}/auth-failed");

                // Assign default role
                var defaultRole = Roles.User.ToString();
                if (!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                }
                var roleAssignResult = await _userManager.AddToRoleAsync(user, defaultRole);
                if (!roleAssignResult.Succeeded)
                    return Redirect($"{_configuration["Frontend:Url"]}/auth-failed");

            }

            var addLogin = await _userManager.AddLoginAsync(user, info);
            if (!addLogin.Succeeded)
                return Redirect($"{_configuration["Frontend:Url"]}/auth-failed");
        }

        var token = await GenerateJwtToken(user);

        var target = $"{_configuration["Frontend:Url"]}/auth-callback?token={token}";
        return Redirect(target);
    }
}
