namespace QuickChart.API.Domain.Entities
{
    public class ChatGroup
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ICollection<GroupMember> Members { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
