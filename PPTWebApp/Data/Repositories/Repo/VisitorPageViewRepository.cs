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

        public void LogPageView(VisitorPageView pageView)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO visitorpageviews (sessionid, pageurl, viewedat, referrer) 
                    VALUES (@SessionId, @PageUrl, @ViewedAt, @Referrer)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SessionId", pageView.SessionId);
                    command.Parameters.AddWithValue("@PageUrl", pageView.PageUrl);
                    command.Parameters.AddWithValue("@ViewedAt", pageView.ViewedAt);
                    command.Parameters.AddWithValue("@Referrer", (object?)pageView.Referrer ?? DBNull.Value);

                    command.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<VisitorPageView> GetPageViewsBySessionId(Guid sessionId)
        {
            var pageViews = new List<VisitorPageView>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, sessionid, pageurl, viewedat, referrer 
                    FROM visitorpageviews 
                    WHERE sessionid = @SessionId";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SessionId", sessionId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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

            return pageViews;
        }
    }
}
