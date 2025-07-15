namespace QuickChart.API.Domain.Dto
{
    public class MessageDto
    {
        public string? ReceiverId { get; set; } // optional for group
        public string? GroupId { get; set; } // optional for one to one
        public string Content { get; set; }
    }
}
