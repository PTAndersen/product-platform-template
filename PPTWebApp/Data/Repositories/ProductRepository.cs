using Npgsql;
using PPTWebApp.Data.Models;
using System.Collections.Generic;
using System.ComponentModel.Design;

namespace PPTWebApp.Data.Repositories
{
    //TODO: Optimize "SELECT *" sql statements
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Product Mapping

        private Product MapProductFromReader(NpgsqlDataReader reader)
        {
            ProductCategory? productCategory = GetProductCategory(reader, "categoryid");
            ProductInventory? productInventory = GetProductInventory(reader, "inventoryid");
            Discount? discount = GetDiscount(reader, "discountid");

            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                SKU = reader.GetString(reader.GetOrdinal("SKU")),
                ProductInventory = productInventory,
                ProductCategory = productCategory,
                Price = reader.GetDecimal(reader.GetOrdinal("price")),
                Discount = discount,
                ImageUrl = reader.GetString(reader.GetOrdinal("imageurl")),
                ImageCompromise = reader.GetString(reader.GetOrdinal("imagecompromise"))
            };
        }

        private ProductCategory? GetProductCategory(NpgsqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                return new ProductCategoryRepository(_connectionString)
                            .GetProductCategoryById(reader.GetInt32(reader.GetOrdinal(columnName)));
            }
        }

        private ProductInventory? GetProductInventory(NpgsqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                return new ProductInventoryRepository(_connectionString)
                            .GetProductInventoryById(reader.GetInt32(reader.GetOrdinal(columnName)));
            }
        }

        private Discount? GetDiscount(NpgsqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                return new DiscountRepository(_connectionString)
                            .GetDiscountById(reader.GetInt32(reader.GetOrdinal(columnName)));
            }
        }

        #endregion

        public Product? GetProductById(int id)
        {
            Product? product = null;
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
                            product = MapProductFromReader(reader);
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

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int? categoryId = null;
                        int? inventoryId = null;

                        if (product.ProductCategory != null)
                        {
                            var productCategoryRepo = new ProductCategoryRepository(_connectionString);
                            categoryId = productCategoryRepo.AddProductCategory(product.ProductCategory);
                        }

                        if (product.ProductInventory != null)
                        {
                            var productInventoryRepo = new ProductInventoryRepository(_connectionString);
                            inventoryId = productInventoryRepo.AddProductInventory(product.ProductInventory);
                        }

                        using (var command = new NpgsqlCommand(
                            "INSERT INTO products (name, description, sku, categoryid, inventoryid, price, imageurl, imagecompromise) " +
                            "VALUES (@Name, @Description, @Sku, @CategoryId, @InventoryId, @Price, @Imageurl, @Imagecompromise)", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Name", product.Name);
                            command.Parameters.AddWithValue("@Description", (object)product.Description ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Sku", product.SKU);

                            command.Parameters.AddWithValue("@CategoryId", categoryId.HasValue ? categoryId.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@InventoryId", inventoryId.HasValue ? inventoryId.Value : DBNull.Value);

                            command.Parameters.AddWithValue("@Price", product.Price);
                            command.Parameters.AddWithValue("@Imageurl", product.ImageUrl);
                            command.Parameters.AddWithValue("@Imagecompromise", product.ImageCompromise);

                            command.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
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


        public IEnumerable<Product> GetBestsellers(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT p.id, p.name, p.description, p.sku, p.price, p.imageurl, p.imagecompromise, 
                           p.categoryid, p.inventoryid, p.discountid, p.createdat, p.modifiedat, p.deletedat,
                           SUM(oi.quantity) AS total_quantity_sold
                    FROM products p
                    LEFT JOIN orderitems oi ON p.id = oi.productid
                    LEFT JOIN orderdetails od ON oi.orderid = od.id
                    WHERE p.price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND p.categoryid = @CategoryId" : "") + @"
                    GROUP BY p.id
                    ORDER BY total_quantity_sold DESC
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }


        public IEnumerable<Product> GetNewestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM products
                    WHERE price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                    ORDER BY createdat DESC
                    OFFSET @StartIndex LIMIT @Range
                    ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public IEnumerable<Product> GetOldestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM products
                    WHERE price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                    ORDER BY createdat ASC
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public IEnumerable<Product> GetCheapestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM products
                    WHERE price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                    ORDER BY price ASC
                    OFFSET @StartIndex LIMIT @Range
                    ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public IEnumerable<Product> GetMostExpensiveProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT *
                    FROM products
                    WHERE price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                    ORDER BY price DESC 
                    OFFSET @StartIndex LIMIT @Range
                    ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }

        public IEnumerable<Product> GetTopDiscountedProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT p.id, p.name, p.description, p.sku, p.price, p.imageurl, p.imagecompromise, p.createdat, p.categoryid, p.inventoryid, p.discountid, d.discountpercent
                    FROM products p
                    JOIN discounts d ON p.discountid = d.id
                    WHERE p.price BETWEEN @MinPrice AND @MaxPrice"
                    + (productCategory != null ? " AND p.categoryid = @CategoryId" : "") + @"
                    ORDER BY d.discountpercent DESC
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }

            return products;
        }


        public IEnumerable<Product> SearchProducts(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            var products = new List<Product>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    SELECT id, name, description, sku, price, imageurl, imagecompromise, createdat, categoryid, inventoryid, discountid
                    FROM products
                    WHERE price BETWEEN @MinPrice AND @MaxPrice"
                    + (string.IsNullOrEmpty(keyword) ? "" : " AND name ILIKE '%' || @Keyword || '%'")
                    + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                    ORDER BY LENGTH(name) ASC
                    OFFSET @StartIndex LIMIT @Range";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@Keyword", keyword);
                    }
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    command.Parameters.AddWithValue("@StartIndex", startIndex);
                    command.Parameters.AddWithValue("@Range", range);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = MapProductFromReader(reader);
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }


        public int GetTotalProductCount(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice)
        {
            int totalCount = 0;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM products WHERE price BETWEEN @MinPrice AND @MaxPrice"
                                + (string.IsNullOrEmpty(keyword) ? "" : " AND name ILIKE '%' || @Keyword || '%'")
                                + (productCategory != null ? " AND categoryid = @CategoryId" : "");
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MinPrice", minPrice);
                    command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        command.Parameters.AddWithValue("@Keyword", keyword);
                    }
                    if (productCategory != null)
                    {
                        command.Parameters.AddWithValue("@CategoryId", productCategory.Id);
                    }
                    totalCount = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            return totalCount;
        }
    }
}
