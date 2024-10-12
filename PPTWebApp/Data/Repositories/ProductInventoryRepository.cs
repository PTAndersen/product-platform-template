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

        public int AddProductInventory(ProductInventory productInventory)
        {
            if (productInventory == null) throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");
            

            string insertSql = @"INSERT INTO productinventories (quantity, createdat) VALUES (@Quantity, @CreatedAt) RETURNING id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Quantity", productInventory.Quantity);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                    object? result = command.ExecuteScalar();
                    int? newId = result as int?;

                    if (newId == null)
                    {
                        throw new InvalidOperationException("Failed to insert record or retrieve the new ID.");
                    }

                    return newId.Value;
                }
            }
        }

        public bool DeleteProductInventory(int id)
        {

            string deleteSql = @"DELETE FROM productinventories WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }


        public ProductInventory? GetProductInventoryById(int id)
        {
            string selectSql = @"SELECT id, quantity FROM productinventories WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(selectSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
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


        public bool UpdateProductInventory(ProductInventory productInventory)
        {
            if (productInventory == null) throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");
            string updateSql = @"
                UPDATE productinventories
                SET quantity = @Quantity
                WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", productInventory.Id);
                    command.Parameters.AddWithValue("@Quantity", productInventory.Quantity);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    }
}
