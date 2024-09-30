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

        public IEnumerable<Product> GetProductsByCategory(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetBestsellers(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            //TODO: actually get best sellers and not just the 
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

        public IEnumerable<Product> GetNewestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetOldestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetCheapestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetMostExpensiveProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetPhysicalProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetSoftwareProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetTopDiscountedProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetInStockProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> SearchProducts(string categoryName, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            throw new NotImplementedException();
        }

        public int GetTotalProductCount(string categoryName)
        {
            int totalCount = 0;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query;

                if(categoryName == "all")
                {
                    query = "SELECT COUNT(*) FROM products";
                } else
                {
                    //TODO: get count by category
                    query = "SELECT COUNT(*) FROM products";
                }
                

                using (var command = new NpgsqlCommand(query, connection))
                {
                    totalCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }

            return totalCount;
        }
    }
}
