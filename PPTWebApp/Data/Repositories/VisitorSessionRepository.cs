using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using Npgsql;

namespace PPTWebApp.Data.Repositories
{
    public class VisitorSessionRepository : IVisitorSessionRepository
    {
        private readonly string _connectionString;

        public VisitorSessionRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task CreateSessionAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                INSERT INTO visitorsessions (sessionid, startedat, endedat) 
                VALUES (@SessionId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '16 hours')";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", sessionId);
                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                // TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating session: {ex.Message}");
                // TODO: Log error
                throw;
            }
        }

        public async Task<List<int>> GetDailyVisitorCountsAsync(int daysBack, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dailyVisitorCounts = new List<int>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        WITH date_series AS (
                            SELECT 
                                CURRENT_DATE - INTERVAL '1 day' * generate_series(0, @DaysBack - 1) AS day
                        )
                        SELECT 
                            day, 
                            COALESCE(COUNT(v.sessionid), 0) AS visitor_count
                        FROM 
                            date_series d
                        LEFT JOIN 
                            visitorsessions v ON d.day = v.startedat::date
                        GROUP BY 
                            day
                        ORDER BY 
                            day ASC";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@DaysBack", daysBack);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                dailyVisitorCounts.Add(reader.GetInt32(1));
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                // TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving daily visitor counts: {ex.Message}");
                // TODO: Log error
                throw;
            }

            return dailyVisitorCounts;
        }

        public async Task<VisitorSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            VisitorSession? session = null;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                SELECT id, sessionid, startedat, endedat 
                FROM visitorsessions 
                WHERE sessionid = @SessionId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", sessionId);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await reader.ReadAsync(cancellationToken))
                            {
                                session = new VisitorSession
                                {
                                    Id = reader.GetInt32(0),
                                    SessionId = reader.GetGuid(1),
                                    StartedAt = reader.GetDateTime(2),
                                    EndedAt = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3)
                                };
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                // TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving session: {ex.Message}");
                // TODO: Log error
                throw;
            }

            return session;
        }

        public async Task<bool> IsSessionValidAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = "SELECT endedat FROM visitorsessions WHERE sessionid = @SessionId";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", sessionId);
                        var endedAt = await command.ExecuteScalarAsync(cancellationToken) as DateTime?;

                        if (!endedAt.HasValue || endedAt.Value <= DateTime.UtcNow)
                        {
                            return false;
                        }

                        return true;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
                // TODO: Log error
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking session validity: {ex.Message}");
                // TODO: Log error
                throw;
            }
        }
    }
}
