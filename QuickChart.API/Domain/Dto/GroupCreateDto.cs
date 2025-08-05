using System.Xml.Linq;

namespace QuickChart.API.Domain.Dto
{
    public class GroupCreateDto
    {
        public string name { get; set; } = string.Empty;
        public List<string>? memberIds { get; set; } = new List<string>();
    }
}