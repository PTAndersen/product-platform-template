using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public IEnumerable<Post> GetHighlightedPosts()
        {
            return _postRepository.GetPostsInRange(null, 0, 3);
        }

        public IEnumerable<Post> GetPostsInRange(string? keyword, int startIndex, int range)
        {
            return _postRepository.GetPostsInRange(keyword, startIndex, range);
        }

        public int GetTotalPostCount(string? keyword)
        {
            return _postRepository.GetTotalPostCount(keyword);
        }

        public Post? GetPostById(int id)
        {
            return _postRepository.GetPostById(id);
        }

        public void AddPost(Post post)
        {
            _postRepository.AddPost(post);
        }

        public void UpdatePost(Post post)
        {
            _postRepository.UpdatePost(post);
        }

        public void DeletePost(int id)
        {
            _postRepository.DeletePost(id);
        }
    }
}
