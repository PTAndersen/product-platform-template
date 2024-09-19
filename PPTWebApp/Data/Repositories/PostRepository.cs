using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;

namespace PPTWebApp.Data.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly string _connectionString;

        public PostRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Post> GetAllPosts()
        {
            var posts = new List<Post>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM Posts", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        posts.Add(new Post
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Content = reader.GetString(2),
                            DatePosted = reader.GetDateTime(3)
                        });
                    }
                }
            }
            return posts;
        }

        public Post GetPostById(int id)
        {
            Post post = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM Posts WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            post = new Post
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.GetString(1),
                                Content = reader.GetString(2),
                                DatePosted = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            return post;
        }

        public void AddPost(Post post)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO Posts (Title, Content, DatePosted) VALUES (@Title, @Content, @DatePosted)", connection))
                {
                    command.Parameters.AddWithValue("@Title", post.Title);
                    command.Parameters.AddWithValue("@Content", post.Content);
                    command.Parameters.AddWithValue("@DatePosted", post.DatePosted);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdatePost(Post post)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE Posts SET Title = @Title, Content = @Content, DatePosted = @DatePosted WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", post.Id);
                    command.Parameters.AddWithValue("@Title", post.Title);
                    command.Parameters.AddWithValue("@Content", post.Content);
                    command.Parameters.AddWithValue("@DatePosted", post.DatePosted);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeletePost(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM Posts WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
