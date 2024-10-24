using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly string _connectionString;

        public UserProfileRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private static UserProfile MapUserProfileFromReader(NpgsqlDataReader reader)
        {
            return new UserProfile
            {
                UserId = reader.GetGuid(reader.GetOrdinal("userid")),
                FirstName = reader.GetString(reader.GetOrdinal("firstname")),
                LastName = reader.GetString(reader.GetOrdinal("lastname")),
                Telephone = reader.IsDBNull(reader.GetOrdinal("telephone")) ? null : reader.GetString(reader.GetOrdinal("telephone")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("createdat")),
                ModifiedAt = reader.IsDBNull(reader.GetOrdinal("modifiedat")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("modifiedat"))
            };
        }

        public async Task<IEnumerable<UserProfile>> GetUserProfilesInRangeAsync(int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userProfiles = new List<UserProfile>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT *
                        FROM userprofiles
                        ORDER BY userid 
                        OFFSET @StartIndex LIMIT @Range";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StartIndex", startIndex);
                        command.Parameters.AddWithValue("@Range", range);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                userProfiles.Add(MapUserProfileFromReader(reader));
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
                Console.WriteLine($"Error retrieving user profiles: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return userProfiles;
        }

        public async Task<int> GetTotalUserProfileCountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int totalCount = 0;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = "SELECT COUNT(*) FROM userprofiles";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
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
                Console.WriteLine($"Error getting total user profile count: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return totalCount;
        }

        public async Task<UserProfile?> GetUserProfileByIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UserProfile? userProfile = null;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand("SELECT * FROM userprofiles WHERE userid = @UserId", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await reader.ReadAsync(cancellationToken))
                            {
                                userProfile = MapUserProfileFromReader(reader);
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
                Console.WriteLine($"Error getting user profile by ID: {ex.Message}");
                //TODO: Log error
                throw;
            }

            return userProfile;
        }

        public async Task AddUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(
                        "INSERT INTO userprofiles (userid, firstname, lastname, telephone, createdat, modifiedat) VALUES (@UserId, @FirstName, @LastName, @Telephone, @CreatedAt, @ModifiedAt)", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userProfile.UserId);
                        command.Parameters.AddWithValue("@FirstName", userProfile.FirstName);
                        command.Parameters.AddWithValue("@LastName", userProfile.LastName);
                        command.Parameters.AddWithValue("@Telephone", (object?)userProfile.Telephone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", userProfile.CreatedAt);
                        command.Parameters.AddWithValue("@ModifiedAt", (object?)userProfile.ModifiedAt ?? DBNull.Value);

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
                Console.WriteLine($"Error adding user profile: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task UpdateUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(
                        "UPDATE userprofiles SET firstname = @FirstName, lastname = @LastName, telephone = @Telephone, modifiedat = @ModifiedAt WHERE userid = @UserId", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userProfile.UserId);
                        command.Parameters.AddWithValue("@FirstName", userProfile.FirstName);
                        command.Parameters.AddWithValue("@LastName", userProfile.LastName);
                        command.Parameters.AddWithValue("@Telephone", (object?)userProfile.Telephone ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ModifiedAt", (object?)userProfile.ModifiedAt ?? DBNull.Value);

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
                Console.WriteLine($"Error updating user profile: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand("DELETE FROM userprofiles WHERE userid = @UserId", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
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
                Console.WriteLine($"Error deleting user profile: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }
    }
}
