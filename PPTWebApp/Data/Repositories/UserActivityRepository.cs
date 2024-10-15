using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using Npgsql;

namespace PPTWebApp.Data.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly string _connectionString;

        public UserActivityRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<bool> UpdateUserActivityAsync(Guid userId, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellation);

                    string checkUserQuery = "SELECT COUNT(1) FROM aspnetusers WHERE id = @UserId";
                    using (var checkCommand = new NpgsqlCommand(checkUserQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@UserId", userId);

                        var result = await checkCommand.ExecuteScalarAsync(cancellation);
                        var userExists = result != DBNull.Value && result != null ? (long)result : 0;

                        if (userExists == 0)
                        {
                            return false;
                        }
                    }

                    string query = @"
                        INSERT INTO useractivity (userid, lastactivityat) 
                        VALUES (@UserId, @LastActivityAt)
                        ON CONFLICT (userid) DO UPDATE 
                        SET lastactivityat = @LastActivityAt";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@LastActivityAt", DateTime.UtcNow);

                        await command.ExecuteNonQueryAsync(cancellation);
                    }

                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                //TODO: Log error
                return false;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error occurred: {ex.Message}");
                //TODO: Log error
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                //TODO: Log error
                return false;
            }
        }

        public async Task<UserActivity?> GetLastActivityAsync(int userId, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellation);

                    string query = @"
                        SELECT id, userid, lastactivityat 
                        FROM useractivity 
                        WHERE userid = @UserId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = await command.ExecuteReaderAsync(cancellation))
                        {
                            if (await reader.ReadAsync(cancellation))
                            {
                                return new UserActivity
                                {
                                    Id = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1),
                                    LastActivityAt = reader.GetDateTime(2)
                                };
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
                Console.WriteLine($"Error retrieving user activity: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return null;
        }

        public async Task<IEnumerable<UserActivity>> GetActiveUsersWithinAsync(TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var activeUsers = new List<UserActivity>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT id, userid, lastactivityat 
                        FROM useractivity 
                        WHERE lastactivityat >= CURRENT_TIMESTAMP - @TimeSpan::interval";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TimeSpan", timeSpan);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                activeUsers.Add(new UserActivity
                                {
                                    Id = reader.GetInt32(0),
                                    UserId = reader.GetInt32(1),
                                    LastActivityAt = reader.GetDateTime(2)
                                });
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
                Console.WriteLine($"Error retrieving active users: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return activeUsers;
        }
    }
}
