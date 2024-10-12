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

        public IEnumerable<ProductCategory> GetAll()
        {
            var productCategories = new List<ProductCategory>();

            string selectSql = @"SELECT id, name, description FROM productcategories;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(selectSql, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
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

            return productCategories;
        }


        public int AddProductCategory(ProductCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category), "Product category cannot be null.");

            string insertSql = @"INSERT INTO productcategories (name, description) 
                                VALUES (@Name, @Description) 
                                RETURNING id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new  NpgsqlCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Name);

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

        public bool DeleteProductCategory(int id)
        {
            string deleteSql = @"DELETE FROM productioncategories WHERE id = @Id;";

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

        public ProductCategory? GetProductCategoryById(int id)
        {
            string selectSql = @"SELECT id, name, description FROM productcategories WHERE id=@Id;";

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

        public bool UpdateProductCategory(ProductCategory category)
        {
            if (category == null) throw new ArgumentNullException(nameof(category), "Product category cannot be null.");

            string updateSql = @"UPDATE productcategories
                                SET name = @Name,
                                    description = @Description
                                WHERE id = @Id;";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Description", category.Description);

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
