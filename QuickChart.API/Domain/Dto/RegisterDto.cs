using QuickChart.API.Helper.Enums;

namespace QuickChart.API.Domain.Dto
{
    public class RegisterDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public UserTypes? UserType { get; set; }  = UserTypes.User; // Default to User type
    }
}
