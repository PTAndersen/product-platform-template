using PPTWebApp.Data.Models;


namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetPostsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken);
        Task<int> GetTotalPostCountAsync(string? keyword, CancellationToken cancellationToken);
        Task<Post?> GetPostByIdAsync(int id, CancellationToken cancellationToken);
        Task AddPostAsync(Post post, CancellationToken cancellationToken);
        Task UpdatePostAsync(Post post, CancellationToken cancellationToken);
        Task DeletePostAsync(int id, CancellationToken cancellationToken);
    }
}
