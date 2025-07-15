namespace QuickChart.API.Domain.Entities
{
    public class GroupMember
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }

        public ApplicationUser User { get; set; }
        public ChatGroup Group { get; set; }
    }
}
