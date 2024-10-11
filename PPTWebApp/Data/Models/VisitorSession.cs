namespace PPTWebApp.Data.Models
{
    public class VisitorSession
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
    }
}
