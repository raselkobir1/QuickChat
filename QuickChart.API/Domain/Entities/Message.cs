namespace QuickChart.API.Domain.Entities
{
    public class Message
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public string? SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public string? ReceiverId { get; set; } // For 1-1 chat
        public ApplicationUser? Receiver { get; set; }

        public string? GroupId { get; set; } // For group chat
        public ChatGroup? Group { get; set; }
    }
}
