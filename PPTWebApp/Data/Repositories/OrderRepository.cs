using Npgsql;

public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<List<decimal>> GetDailySalesAsync(int daysBack)
    {
        var dailySales = new List<decimal>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Create a series of dates and join with the actual sales data
            string query = $@"
                WITH date_series AS (
                    SELECT 
                        CURRENT_DATE - INTERVAL '1 day' * generate_series(0, @DaysBack - 1) AS day
                )
                SELECT 
                    day, 
                    COALESCE(SUM(o.total), 0) AS daily_sales
                FROM 
                    date_series d
                LEFT JOIN 
                    orderdetails o ON d.day = o.createdat::date
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
                        dailySales.Add(reader.GetDecimal(1));
                    }
                }
            }
        }

        return dailySales;
    }
}
