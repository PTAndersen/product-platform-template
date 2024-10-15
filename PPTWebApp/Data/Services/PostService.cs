using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<IEnumerable<Post>> GetHighlightedPostsAsync(CancellationToken cancellationToken)
        {
            return await _postRepository.GetPostsInRangeAsync(null, 0, 3, cancellationToken);
        }

        public async Task<IEnumerable<Post>> GetPostsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _postRepository.GetPostsInRangeAsync(keyword, startIndex, range, cancellationToken);
        }

        public async Task<int> GetTotalPostCountAsync(string? keyword, CancellationToken cancellationToken)
        {
            return await _postRepository.GetTotalPostCountAsync(keyword, cancellationToken);
        }

        public async Task<Post?> GetPostByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _postRepository.GetPostByIdAsync(id, cancellationToken);
        }

        public async Task AddPostAsync(Post post, CancellationToken cancellationToken)
        {
            await _postRepository.AddPostAsync(post, cancellationToken);
        }

        public async Task UpdatePostAsync(Post post, CancellationToken cancellationToken)
        {
            await _postRepository.UpdatePostAsync(post, cancellationToken);
        }

        public async Task DeletePostAsync(int id, CancellationToken cancellationToken)
        {
            await _postRepository.DeletePostAsync(id, cancellationToken);
        }
    }
}
