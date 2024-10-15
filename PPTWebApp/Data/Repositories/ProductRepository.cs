using Npgsql;
using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
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

        private async Task<Product> MapProductFromReaderAsync(NpgsqlDataReader reader, CancellationToken cancellationToken)
        {
            ProductCategory? productCategory = await GetProductCategoryAsync(reader, "categoryid", cancellationToken);
            ProductInventory? productInventory = await GetProductInventoryAsync(reader, "inventoryid", cancellationToken);
            Discount? discount = await GetDiscountAsync(reader, "discountid", cancellationToken);

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

        private async Task<ProductCategory?> GetProductCategoryAsync(NpgsqlDataReader reader, string columnName, CancellationToken cancellationToken)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                int productCategoryId = reader.GetInt32(reader.GetOrdinal(columnName));
                return await new ProductCategoryRepository(_connectionString).GetProductCategoryByIdAsync(productCategoryId, cancellationToken);
            }
        }

        private async Task<ProductInventory?> GetProductInventoryAsync(NpgsqlDataReader reader, string columnName, CancellationToken cancellationToken)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                int productInventoryId = reader.GetInt32(reader.GetOrdinal(columnName));
                return await new ProductInventoryRepository(_connectionString).GetProductInventoryByIdAsync(productInventoryId, cancellationToken);
            }
        }

        private async Task<Discount?> GetDiscountAsync(NpgsqlDataReader reader, string columnName, CancellationToken cancellationToken)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return null;
            }
            else
            {
                int discountId = reader.GetInt32(reader.GetOrdinal(columnName));
                return await new DiscountRepository(_connectionString).GetDiscountByIdAsync(discountId, cancellationToken);
            }
        }


        #endregion

        public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Product? product = null;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    using (var command = new NpgsqlCommand("SELECT * FROM products WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            if (await reader.ReadAsync(cancellationToken))
                            {
                                product = await MapProductFromReaderAsync(reader, cancellationToken);
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

            return product;
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
                {
                    try
                    {
                        int? categoryId = null;
                        int? inventoryId = null;

                        if (product.ProductCategory != null)
                        {
                            var productCategoryRepo = new ProductCategoryRepository(_connectionString);
                            categoryId = await productCategoryRepo.AddProductCategoryAsync(product.ProductCategory, cancellationToken);
                        }

                        if (product.ProductInventory != null)
                        {
                            var productInventoryRepo = new ProductInventoryRepository(_connectionString);
                            inventoryId = await productInventoryRepo.AddProductInventoryAsync(product.ProductInventory, cancellationToken);
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

                            await command.ExecuteNonQueryAsync(cancellationToken);
                        }

                        await transaction.CommitAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation was canceled.");
                        //TODO: Log error
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        Console.WriteLine($"Error getting all discounts in range: {ex.Message}");
                        //TODO: Log error
                        throw;
                    }
                }
            }
        }



        public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new NpgsqlCommand(
                    "UPDATE products SET name = @name, description = @description, price = @price, imageurl = @imageurl, imagecompromise = @imagecompromise WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", product.Id);
                    command.Parameters.AddWithValue("@name", product.Name);
                    command.Parameters.AddWithValue("@description", (object)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@price", product.Price);
                    command.Parameters.AddWithValue("@imageurl", product.ImageUrl);
                    command.Parameters.AddWithValue("@imagecompromise", product.ImageCompromise);

                    try
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
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

        public async Task DeleteProductAsync(int id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = new NpgsqlCommand("DELETE FROM products WHERE id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    try
                    {
                        await command.ExecuteNonQueryAsync(cancellationToken);
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

        public async Task<IEnumerable<Product>> GetBestsellersAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }

        public async Task<IEnumerable<Product>> GetNewestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }

        public async Task<IEnumerable<Product>> GetOldestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }

        public async Task<IEnumerable<Product>> GetCheapestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT *
                        FROM products
                        WHERE price BETWEEN @MinPrice AND @MaxPrice"
                                + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                        ORDER BY price ASC
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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }


        public async Task<IEnumerable<Product>> GetMostExpensiveProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT *
                        FROM products
                        WHERE price BETWEEN @MinPrice AND @MaxPrice"
                                + (productCategory != null ? " AND categoryid = @CategoryId" : "") + @"
                        ORDER BY price DESC 
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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }


        public async Task<IEnumerable<Product>> GetTopDiscountedProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }



        public async Task<IEnumerable<Product>> SearchProductsAsync(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var products = new List<Product>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);
                                products.Add(product);
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

            return products;
        }



        public async Task<int> GetTotalProductCountAsync(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int totalCount = 0;

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

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

                        totalCount = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
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

            return totalCount;
        }


        public async Task<List<(int Sales, Product Product)>> GetTopSellingProductsAsync(int topProductsCount, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var topSellingProducts = new List<(int Sales, Product Product)>();

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = $@"
                        SELECT 
                            p.id, p.name, p.description, p.SKU, p.price, p.imageurl, p.imagecompromise,
                            p.categoryid, p.inventoryid, p.discountid,  -- Include required columns
                            COALESCE(SUM(oi.quantity), 0) AS sales
                        FROM 
                            products p
                        LEFT JOIN 
                            orderitems oi ON p.id = oi.productid
                        LEFT JOIN 
                            orderdetails od ON oi.orderid = od.id
                        WHERE 
                            p.deletedat IS NULL  -- Ensure you're only fetching non-deleted products
                        GROUP BY 
                            p.id
                        ORDER BY 
                            sales DESC
                        LIMIT @TopProductsCount";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TopProductsCount", topProductsCount);

                        using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                        {
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                var product = await MapProductFromReaderAsync(reader, cancellationToken);

                                int sales = reader.GetInt32(reader.GetOrdinal("sales"));
                                topSellingProducts.Add((sales, product));
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

            return topSellingProducts;
        }
    }
}
