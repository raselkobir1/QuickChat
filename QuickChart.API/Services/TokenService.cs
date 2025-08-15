using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuickChart.API.Domain;
using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace QuickChart.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(
            AppDbContext context,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
        }
        public async Task<string> GenerateAccessToken(ApplicationUser user)
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
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<TokenResponse> RefreshTokenAsync(string existingRefreshToken, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == existingRefreshToken);

            if (refreshToken == null || refreshToken.Expiration <= DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            // Generate new tokens
            var accessToken  = await GenerateAccessToken(refreshToken.User);
            var newRefreshToken = GenerateRefreshToken();  

            // Update existing token
            refreshToken.Token = newRefreshToken;
            refreshToken.CreatedByIp = ipAddress;
            refreshToken.UserId = refreshToken.User.Id;

            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }
        

        public async Task RevokeTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
                throw new SecurityTokenException("Invalid refresh token");

            _context.RefreshTokens.Remove(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task SaveTokenAsync(string newRefreshToken, string userId)
        {
            if (newRefreshToken == null)
                throw new ArgumentNullException(nameof(newRefreshToken));

            var refreshToken = await _context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);

            if (refreshToken == null)
            {
                var token = new RefreshToken { UserId = userId, Token = newRefreshToken };
                await _context.AddAsync(token);
                await _context.SaveChangesAsync();
            }
            else 
            {
                refreshToken.Token = newRefreshToken;

                _context.RefreshTokens.Update(refreshToken);
                await _context.SaveChangesAsync();
            }
        }
    }
}
