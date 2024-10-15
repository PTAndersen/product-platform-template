using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Repositories
{
    public class ProductInventoryRepository : IProductInventoryRepository
    {
        private readonly string _connectionString;

        public ProductInventoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> AddProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (productInventory == null) throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");

            string insertSql = @"INSERT INTO productinventories (quantity, createdat) VALUES (@Quantity, @CreatedAt) RETURNING id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@Quantity", productInventory.Quantity);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

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
                Console.WriteLine($"Error adding product inventory: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task<bool> DeleteProductInventoryAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string deleteSql = @"DELETE FROM productinventories WHERE id = @Id;";

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
                Console.WriteLine($"Error deleting product inventory: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task<ProductInventory?> GetProductInventoryByIdAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string selectSql = @"SELECT id, quantity FROM productinventories WHERE id = @Id;";

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
                                var productInventory = new ProductInventory
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("quantity"))
                                };

                                return productInventory;
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
                Console.WriteLine($"Error retrieving product inventory: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }

        public async Task<bool> UpdateProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (productInventory == null) throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");

            string updateSql = @"
                UPDATE productinventories
                SET quantity = @Quantity
                WHERE id = @Id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", productInventory.Id);
                        command.Parameters.AddWithValue("@Quantity", productInventory.Quantity);

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
                Console.WriteLine($"Error updating product inventory: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }
    }
}
