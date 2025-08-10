namespace QuickChart.API.Domain.Dto
{
    public class UpdateProfileDto
    {
        public required string FullName { get; set; }
        public required string Email { get; set; } 
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? ParmanantAddress { get; set; }
        public string? PresentAddress { get; set; }
        public string? PhoneNumber { get; set; } 
        public string? UniversityName { get; set; }
        public string? CollageName { get; set; }
        public string? WorkPlaceName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }
}
