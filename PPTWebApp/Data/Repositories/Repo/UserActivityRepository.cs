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

        public bool UpdateUserActivity(Guid userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string checkUserQuery = "SELECT COUNT(1) FROM aspnetusers WHERE id = @UserId";
                using (var checkCommand = new NpgsqlCommand(checkUserQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@UserId", userId);
                    var userExists = (long)checkCommand.ExecuteScalar();

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

                    command.ExecuteNonQuery();
                }

                return true;
            }
        }

        public UserActivity? GetLastActivity(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, userid, lastactivityat 
                    FROM useractivity 
                    WHERE userid = @UserId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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

            return null;
        }

        public IEnumerable<UserActivity> GetActiveUsersWithin(TimeSpan timeSpan)
        {
            var activeUsers = new List<UserActivity>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, userid, lastactivityat 
                    FROM useractivity 
                    WHERE lastactivityat >= CURRENT_TIMESTAMP - @TimeSpan";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TimeSpan", timeSpan);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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

            return activeUsers;
        }
    }
}
