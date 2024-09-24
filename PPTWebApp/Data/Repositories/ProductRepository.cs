using Npgsql;
using PPTWebApp.Data.Models;
using System.Collections.Generic;

namespace PPTWebApp.Data.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM products", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            ImageUrl = reader.GetString(4),
                            ImageCompromise = reader.GetString(5)
                        });
                    }
                }
            }
            return products;
        }

        public IEnumerable<Product> GetProductsInRange(int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, name, description, price, imageurl, imagecompromise
                    FROM products
                    ORDER BY id
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                ImageUrl = reader.GetString(4),
                                ImageCompromise = reader.GetString(5)
                            });
                        }
                    }
                }
            }

            return products;
        }

        public int GetTotalProductCount()
        {
            int totalCount = 0;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM products";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    totalCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return totalCount;
        }

        public Product GetProductById(int id)
        {
            Product product = null;
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("SELECT * FROM products WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Product
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Price = reader.GetDecimal(3),
                                ImageUrl = reader.GetString(4),
                                ImageCompromise = reader.GetString(5)
                            };
                        }
                    }
                }
            }
            return product;
        }

        public void AddProduct(Product product)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "INSERT INTO products (name, description, price, imageurl, imagecompromise) VALUES (@name, @description, @price, @imageurl, @imagecompromise)", connection))
                {
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@imageurl", product.ImageUrl);
                    command.Parameters.AddWithValue("@imagecompromise", product.ImageCompromise);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProduct(Product product)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(
                    "UPDATE products SET name = @name, description = @description, price = @price, imageurl = @imageurl, imagecompromise = @imagecompromise WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", product.Id);
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@imageurl", product.ImageUrl);
                    command.Parameters.AddWithValue("@imagecompromise", product.ImageCompromise);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand("DELETE FROM products WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
