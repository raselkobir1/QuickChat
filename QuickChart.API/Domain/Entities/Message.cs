namespace QuickChart.API.Domain.Entities
{
    public class Message
    {
        public long Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public long? SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public long? ReceiverId { get; set; } // For 1-1 chat
        public ApplicationUser? Receiver { get; set; }

        public long? GroupId { get; set; } // For group chat
        public ChatGroup? Group { get; set; }
    }
}
