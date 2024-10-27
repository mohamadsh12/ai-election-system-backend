namespace WebApplication16.models
{
    public class ProtocolEntry
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Place { get; set; }
        public string Description { get; set; }
        public bool IsUrgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
