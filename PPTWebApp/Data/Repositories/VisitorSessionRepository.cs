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

        public void CreateSession(Guid sessionId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO visitorsessions (sessionid, startedat, endedat) 
                    VALUES (@SessionId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '16 hours')";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task<List<int>> GetDailyVisitorCountsAsync(int daysBack)
        {
            var dailyVisitorCounts = new List<int>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();

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

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dailyVisitorCounts.Add(reader.GetInt32(1));
                        }
                    }
                }
            }

            return dailyVisitorCounts;
        }


        public VisitorSession? GetSessionById(Guid sessionId)
        {
            VisitorSession? session = null;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, sessionid, startedat, endedat 
                    FROM visitorsessions 
                    WHERE sessionid = @SessionId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SessionId", sessionId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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

            return session;
        }

        public bool IsSessionValid(Guid sessionId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT endedat FROM visitorsessions WHERE sessionid = @SessionId";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SessionId", sessionId);
                    var endedAt = command.ExecuteScalar() as DateTime?;

                    if (!endedAt.HasValue)
                    {
                        return false;
                    }
                    if (endedAt.Value <= DateTime.UtcNow)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }


    }
}
