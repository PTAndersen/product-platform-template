using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly string _connectionString;

        public DiscountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var discounts = new List<Discount>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                    SELECT id, name, description, discountpercent, active
                    FROM discounts"
                    + (string.IsNullOrEmpty(keyword) ? "" : " WHERE name ILIKE '%' || @Keyword || '%'") +
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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync())
                            {
                                discounts.Add(new Discount
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Description = reader.GetString(reader.GetOrdinal("description")),
                                    DiscountPercent = reader.GetDecimal(reader.GetOrdinal("discountpercent")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("active"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
            }

            return discounts;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsInRangeAsync(string? keyword, bool isActive, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var discounts = new List<Discount>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT id, name, description, discountpercent, active
                        FROM discounts
                        WHERE active = @IsActive"
                        + (string.IsNullOrEmpty(keyword) ? "" : " AND name ILIKE '%' || @Keyword || '%'") +
                        @" ORDER BY id 
                        OFFSET @StartIndex LIMIT @Range";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsActive", isActive);

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
                                discounts.Add(new Discount
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Description = reader.GetString(reader.GetOrdinal("description")),
                                    DiscountPercent = reader.GetDecimal(reader.GetOrdinal("discountpercent")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("active"))
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
            }

            return discounts;
        }


        public async Task<int> GetDiscountCountAsync(string? keyword, CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                string query = @"
                    SELECT COUNT(*)
                    FROM discounts"
                    + (string.IsNullOrEmpty(keyword) ? "" : " WHERE name ILIKE '%' || @Keyword || '%'");

                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            command.Parameters.AddWithValue("@Keyword", keyword);
                        }

                        var result = await command.ExecuteScalarAsync(cancellationToken);
                        return Convert.ToInt32(result);
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
                Console.WriteLine($"An error occurred: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }


        public async Task<int> GetDiscountCountAsync(string? keyword, bool isActive, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string query = @"
                SELECT COUNT(*)
                FROM discounts
                WHERE active = @IsActive"
                + (string.IsNullOrEmpty(keyword) ? "" : " AND name ILIKE '%' || @Keyword || '%'");

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@IsActive", isActive);

                        if (!string.IsNullOrEmpty(keyword))
                        {
                            command.Parameters.AddWithValue("@Keyword", keyword);
                        }

                        var result = await command.ExecuteScalarAsync(cancellationToken);
                        return Convert.ToInt32(result);
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }


        public async Task<int> AddDiscountAsync(Discount discount, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (discount == null) throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");

            string insertSql = @"INSERT INTO discounts (name, description, discountpercent, active)
                                VALUES (@Name, @Description, @Discountpercent, @Active)
                                RETURNING id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", discount.Name);
                        command.Parameters.AddWithValue("@Description", discount.Description);
                        command.Parameters.AddWithValue("@Discountpercent", discount.DiscountPercent);
                        command.Parameters.AddWithValue("@Active", discount.IsActive);

                        object? result = await command.ExecuteScalarAsync(cancellationToken);
                        int? newId = result as int?;

                        if (newId == null)
                        {
                            throw new InvalidOperationException("Failed to insert record or retrieve the new ID.");
                        }

                        return newId.Value;
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }


        public async Task<bool> DeleteDiscountAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string deleteSql = @"DELETE FROM discounts WHERE id = @Id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                        return rowsAffected > 0;
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }


        public async Task<Discount?> GetDiscountByIdAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string selectSql = @"SELECT id, name, description, discountpercent, active FROM discounts WHERE id = @Id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(selectSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await reader.ReadAsync(cancellationToken))
                            {
                                var discount = new Discount
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Description = reader.GetString(reader.GetOrdinal("description")),
                                    DiscountPercent = reader.GetDecimal(reader.GetOrdinal("discountpercent")),
                                    IsActive = reader.GetBoolean(reader.GetOrdinal("active"))
                                };

                                return discount;
                            }
                            else
                            {
                                return null;
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }


        public async Task<bool> UpdateDiscountAsync(Discount discount, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (discount == null) throw new ArgumentNullException(nameof(discount), "Discount cannot be null.");

            string updateSql = @"UPDATE discounts
                        SET name = @Name,
                            description = @Description,
                            discountpercent = @Discountpercent,
                            active = @Active
                        WHERE id = @Id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", discount.Name);
                        command.Parameters.AddWithValue("@Description", discount.Description);
                        command.Parameters.AddWithValue("@Discountpercent", discount.DiscountPercent);
                        command.Parameters.AddWithValue("@Active", discount.IsActive);
                        command.Parameters.AddWithValue("@Id", discount.Id);

                        int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

                        return rowsAffected > 0;
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
                Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

    }
}
