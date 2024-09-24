using PPTWebApp.Data.Models;


namespace PPTWebApp.Data.Repositories
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetAllPosts();
        IEnumerable<Post> GetPostsInRange(int startIndex, int range);
        int GetTotalPostCount();
        Post GetPostById(int id);
        void AddPost(Post post);
        void UpdatePost(Post post);
        void DeletePost(int id);
    }
}
