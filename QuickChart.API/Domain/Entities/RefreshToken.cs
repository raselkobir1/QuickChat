namespace QuickChart.API.Domain.Entities
{
    public class RefreshToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsRevoked { get; set; } = false;
        public bool IsActive => !IsRevoked && Expiration > DateTime.UtcNow;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; } = null;
        public string? CreatedByIp { get; set; } = null;

        public ApplicationUser User { get; set; }
    }
}
