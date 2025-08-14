using QuickChart.API.Domain.Dto;
using QuickChart.API.Domain.Entities;

namespace QuickChart.API.Services
{
    public interface ITokenService
    {
        Task<string> GenerateAccessToken(ApplicationUser user);
        string GenerateRefreshToken();
        Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task RevokeTokenAsync(string token, string ipAddress); 
    }
}
