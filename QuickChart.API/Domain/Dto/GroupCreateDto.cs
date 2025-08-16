using System.Xml.Linq;

namespace QuickChart.API.Domain.Dto
{
    public class GroupCreateDto
    {
        public required string Name { get; set; } = string.Empty; 
        public List<string>? MemberIds { get; set; } = new List<string>();
    }
}