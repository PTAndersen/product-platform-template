using Npgsql;
using PPTWebApp.Components.Pages;
using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly string _connectionString;

        public PostRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static Post MapPostFromReader(NpgsqlDataReader reader)
        {
            return new Post
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Content = reader.GetString(reader.GetOrdinal("content")),           
                ImageUrl = reader.GetString(reader.GetOrdinal("imageurl")),
                ImageCompromise = reader.GetString(reader.GetOrdinal("imagecompromise")),
                DatePosted = reader.GetDateTime(reader.GetOrdinal("dateposted"))
            };
        }

        public IEnumerable<Post> GetPostsInRange(string? keyword, int startIndex, int range)
        {
            var posts = new List<Post>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM posts"
                    + (string.IsNullOrEmpty(keyword) ? "" : " WHERE title ILIKE '%' || @Keyword || '%'") +
                    @" ORDER BY id 
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@Keyword", keyword);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            posts.Add(MapPostFromReader(reader));
                        }
                    }
                }
            }

            return posts;
        }

        public int GetTotalPostCount(string? keyword)
        {
            int totalCount = 0;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM posts"
                    + (string.IsNullOrEmpty(keyword) ? "" : " WHERE title ILIKE '%' || @Keyword || '%'");

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@Keyword", keyword);
                    }
                    totalCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return totalCount;
        }

        public Post? GetPostById(int id)
        {
            Post? post = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM posts WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            post = MapPostFromReader(reader);
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
                    "INSERT INTO posts (title, content, imageurl, imagecompromise, dateposted) VALUES (@Title, @Content, @ImageUrl, @ImageCompromise, @DatePosted)", connection))
                {
                    command.Parameters.AddWithValue("@Title", post.Title);
                    command.Parameters.AddWithValue("@Content", post.Content);
                    command.Parameters.AddWithValue("@ImageUrl", post.ImageUrl);
                    command.Parameters.AddWithValue("@ImageCompromise", post.ImageCompromise);
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
                    "UPDATE posts SET title = @Title, content = @Content, imageurl = @ImageUrl, imagecompromise = @ImageCompromise, dateposted = @DatePosted WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", post.Id);
                    command.Parameters.AddWithValue("@Title", post.Title);
                    command.Parameters.AddWithValue("@Content", post.Content);
                    command.Parameters.AddWithValue("@ImageUrl", post.ImageUrl);
                    command.Parameters.AddWithValue("@ImageCompromise", post.ImageCompromise);
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
                using (var command = new NpgsqlCommand("DELETE FROM posts WHERE id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
