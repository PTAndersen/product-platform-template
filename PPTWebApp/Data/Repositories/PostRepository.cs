using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

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

        public async Task<IEnumerable<Post>> GetPostsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var posts = new List<Post>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT *
                        FROM posts"
                        + (string.IsNullOrEmpty(keyword) ? "" : " WHERE title ILIKE '%' || @Keyword || '%'") +
                        @" ORDER BY dateposted DESC 
                        OFFSET @StartIndex LIMIT @Range";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            command.Parameters.AddWithValue("@Keyword", keyword);
                        }
                        command.Parameters.AddWithValue("@StartIndex", startIndex);
                        command.Parameters.AddWithValue("@Range", range);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                posts.Add(MapPostFromReader(reader));
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving posts: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return posts;
        }

        public async Task<int> GetTotalPostCountAsync(string? keyword, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int totalCount = 0;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = "SELECT COUNT(*) FROM posts"
                        + (string.IsNullOrEmpty(keyword) ? "" : " WHERE title ILIKE '%' || @Keyword || '%'");

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            command.Parameters.AddWithValue("@Keyword", keyword);
                        }
                        totalCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting total post count: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return totalCount;
        }

        public async Task<Post?> GetPostByIdAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Post? post = null;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand("SELECT * FROM posts WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await reader.ReadAsync(cancellationToken))
                            {
                                post = MapPostFromReader(reader);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting post by ID: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return post;
        }

        public async Task AddPostAsync(Post post, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(
                        "INSERT INTO posts (title, content, imageurl, imagecompromise, dateposted) VALUES (@Title, @Content, @ImageUrl, @ImageCompromise, @DatePosted)", connection))
                    {
                        command.Parameters.AddWithValue("@Title", post.Title);
                        command.Parameters.AddWithValue("@Content", post.Content);
                        command.Parameters.AddWithValue("@ImageUrl", post.ImageUrl);
                        command.Parameters.AddWithValue("@ImageCompromise", post.ImageCompromise);
                        command.Parameters.AddWithValue("@DatePosted", post.DatePosted);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding post: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task UpdatePostAsync(Post post, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(
                        "UPDATE posts SET title = @Title, content = @Content, imageurl = @ImageUrl, imagecompromise = @ImageCompromise, dateposted = @DatePosted WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", post.Id);
                        command.Parameters.AddWithValue("@Title", post.Title);
                        command.Parameters.AddWithValue("@Content", post.Content);
                        command.Parameters.AddWithValue("@ImageUrl", post.ImageUrl);
                        command.Parameters.AddWithValue("@ImageCompromise", post.ImageCompromise);
                        command.Parameters.AddWithValue("@DatePosted", post.DatePosted);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task DeletePostAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand("DELETE FROM posts WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting post: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }
    }
}
