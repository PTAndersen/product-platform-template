using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using Npgsql;

namespace PPTWebApp.Data.Repositories
{
    public class HighlightRepository : IHighlightRepository
    {
        private readonly string _connectionString;
        private readonly IProductRepository _productRepository;

        public HighlightRepository(string connectionString, IProductRepository productRepository)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public async Task<bool> AddHighlightAsync(int productId, int position, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (position < 1 || position > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 1 and 3.");
            }

            var product = await _productRepository.GetProductByIdAsync(productId, cancellationToken);
            if (product == null)
            {
                return false;
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        INSERT INTO producthighlights (productid, position, createdat) 
                        VALUES (@ProductId, @Position, CURRENT_TIMESTAMP) 
                        ON CONFLICT (position) DO UPDATE SET productid = EXCLUDED.productid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductId", productId);
                        command.Parameters.AddWithValue("@Position", position);

                        await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                }

                return true;
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

        public async Task<IEnumerable<Product?>> GetHighlightsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var highlights = new List<Product?> { null, null, null };

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = @"
                        SELECT productid, position 
                        FROM producthighlights 
                        ORDER BY position ASC";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            int productId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            int position = reader.GetInt32(1);

                            if (position >= 1 && position <= 3)
                            {
                                if (productId != 0)
                                {
                                    var product = await _productRepository.GetProductByIdAsync(productId, cancellationToken);
                                    highlights[position - 1] = product;
                                }
                                else
                                {
                                    highlights[position - 1] = null;
                                }
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

            return highlights;
        }



        public async Task RemoveHighlightAsync(int position, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (position < 1 || position > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 1 and 3.");
            }

            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync(cancellationToken);

                    string query = "UPDATE producthighlights SET productid = NULL WHERE position = @Position";
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Position", position);
                        await command.ExecuteNonQueryAsync(cancellationToken);
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
