namespace PPTWebApp.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
