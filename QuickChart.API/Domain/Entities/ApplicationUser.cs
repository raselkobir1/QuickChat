using Microsoft.AspNetCore.Identity;

namespace QuickChart.API.Domain.Entities
{
    public class ApplicationUser: IdentityUser
    {
        public ICollection<Message> Messages { get; set; }
    }
}
