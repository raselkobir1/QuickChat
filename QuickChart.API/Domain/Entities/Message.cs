using System.ComponentModel.DataAnnotations.Schema;

namespace QuickChart.API.Domain.Entities
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string? UserName { get; set; } = string.Empty; 
        // Foreign Keys
        public string? SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public string? ReceiverId { get; set; } // For 1-1 chat
        public ApplicationUser? Receiver { get; set; }

        public string? GroupId { get; set; } // For group chat
        public ChatGroup? Group { get; set; }
    }
}
