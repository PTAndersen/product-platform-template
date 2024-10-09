namespace PPTWebApp.Data.Models
{
    public class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string ImageUrl { get; set; }
        public required string ImageCompromise {  get; set; }
        public string? Author { get; set; }
        public DateTime DatePosted { get; set; }
    }
}
