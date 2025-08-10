using Microsoft.AspNetCore.Identity;

namespace QuickChart.API.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public string? FullName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? ParmanantAddress { get; set; } 
        public string? PresentAddress { get; set; }  
        public string? UniversityName { get; set; }
        public string? CollageName { get; set; }
        public string? WorkPlaceName { get; set; } 
        public DateOnly? DateOfBirth { get; set; }

        public ICollection<Message> Messages { get; set; } = new List<Message>(); 
    }
}
