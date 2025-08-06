using System.ComponentModel.DataAnnotations;

namespace QuickChart.API.Domain.Dto
{
    public class AssignUserToGroupDto
    {
        [Required]
        public string GroupId { get; set; } = string.Empty;
        [Required]
        public List<string> memberIds { get; set; } = new List<string>();
    }
}

