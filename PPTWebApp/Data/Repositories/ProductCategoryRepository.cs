using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly string _connectionString;

        public ProductCategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        public async Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var productCategories = new List<ProductCategory>();

            string selectSql = @"SELECT id, name, description FROM productcategories;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(selectSql, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var productCategory = new ProductCategory
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Description = reader.GetString(reader.GetOrdinal("description"))
                                };

                                productCategories.Add(productCategory);
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

            return productCategories;
        }

        public async Task<int> AddProductCategoryAsync(ProductCategory category, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (category == null) throw new ArgumentNullException(nameof(category), "Product category cannot be null.");

            string insertSql = @"INSERT INTO productcategories (name, description) 
                                VALUES (@Name, @Description) 
                                RETURNING id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(insertSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@Description", category.Description);

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

        public async Task<bool> DeleteProductCategoryAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string deleteSql = @"DELETE FROM productcategories WHERE id = @Id;";

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

        public async Task<ProductCategory?> GetProductCategoryByIdAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string selectSql = @"SELECT id, name, description FROM productcategories WHERE id=@Id;";

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
                                var productCategory = new ProductCategory
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    Name = reader.GetString(reader.GetOrdinal("name")),
                                    Description = reader.GetString(reader.GetOrdinal("description")),
                                };

                                return productCategory;
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

        public async Task<bool> UpdateProductCategoryAsync(ProductCategory category, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (category == null) throw new ArgumentNullException(nameof(category), "Product category cannot be null.");

            string updateSql = @"UPDATE productcategories
                        SET name = @Name,
                            description = @Description
                        WHERE id = @Id;";

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", category.Name);
                        command.Parameters.AddWithValue("@Description", category.Description);
                        command.Parameters.AddWithValue("@Id", category.Id);

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
                Console.WriteLine($"Error updating product category: {ex.Message}");
                //TODO: Log error
                throw;
            }
        }
    }
}
