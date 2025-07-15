namespace QuickChart.API.Domain.Entities
{
    public class ChatGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; }

        public ICollection<GroupMember> Members { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
