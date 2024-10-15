using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using Npgsql;
using System.Collections.Generic;

namespace PPTWebApp.Data.Repositories
{
    public class VisitorPageViewRepository : IVisitorPageViewRepository
    {
        private readonly string _connectionString;

        public VisitorPageViewRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task LogPageViewAsync(VisitorPageView pageView, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        INSERT INTO visitorpageviews (sessionid, pageurl, viewedat, referrer) 
                        VALUES (@SessionId, @PageUrl, @ViewedAt, @Referrer)";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", pageView.SessionId);
                        command.Parameters.AddWithValue("@PageUrl", pageView.PageUrl);
                        command.Parameters.AddWithValue("@ViewedAt", pageView.ViewedAt);
                        command.Parameters.AddWithValue("@Referrer", (object?)pageView.Referrer ?? DBNull.Value);

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
                Console.WriteLine($"Error logging page view: {ex.Message}");
                // TODO: Log error
                throw;
            }
        }

        public async Task<IEnumerable<VisitorPageView>> GetPageViewsBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var pageViews = new List<VisitorPageView>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT id, sessionid, pageurl, viewedat, referrer 
                        FROM visitorpageviews 
                        WHERE sessionid = @SessionId";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SessionId", sessionId);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var pageView = new VisitorPageView
                                {
                                    Id = reader.GetInt32(0),
                                    SessionId = reader.GetGuid(1),
                                    PageUrl = reader.GetString(2),
                                    ViewedAt = reader.GetDateTime(3),
                                    Referrer = reader.IsDBNull(4) ? null : reader.GetString(4)
                                };
                                pageViews.Add(pageView);
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
                Console.WriteLine($"Error retrieving page views: {ex.Message}");
                // TODO: Log error
                throw;
            }

            return pageViews;
        }

    }
}
