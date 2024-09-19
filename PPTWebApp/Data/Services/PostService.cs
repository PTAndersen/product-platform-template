using System.Collections.Generic;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;

namespace PPTWebApp.Data.Services
{
    public class PostService
    {
        private readonly IPostRepository _postRepository;

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public IEnumerable<Post> GetAllPosts()
        {
            return _postRepository.GetAllPosts();
        }

        public Post GetPostById(int id)
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
