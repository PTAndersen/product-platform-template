namespace PPTWebApp.Data.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LastActivityAt { get; set; }
    }
}
