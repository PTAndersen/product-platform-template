using PPTWebApp.Data.Models;


namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetPostsInRange(string? keyword, int startIndex, int range);
        int GetTotalPostCount(string? keyword);
        Post? GetPostById(int id);
        void AddPost(Post post);
        void UpdatePost(Post post);
        void DeletePost(int id);
    }
}
