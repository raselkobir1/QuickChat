namespace QuickChart.API.Domain.Entities
{
    public class GroupMember
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string GroupId { get; set; }

        public ApplicationUser User { get; set; }
        public ChatGroup Group { get; set; }
    }
}
