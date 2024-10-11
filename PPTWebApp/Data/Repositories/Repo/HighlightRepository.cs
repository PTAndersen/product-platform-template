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

        public bool AddHighlight(int productId, int position)
        {
            if (position < 1 || position > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 1 and 3.");
            }

            var product = _productRepository.GetProductById(productId);
            if (product == null)
            {
                return false; 
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = @"
                    INSERT INTO producthighlights (productid, position, createdat) 
                    VALUES (@ProductId, @Position, CURRENT_TIMESTAMP) 
                    ON CONFLICT (position) DO UPDATE SET productid = EXCLUDED.productid";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@Position", position);

                    command.ExecuteNonQuery();
                }
            }

            return true;
        }


        public IEnumerable<Product?> GetHighlights()
        {
            var highlights = new List<Product?> { null, null, null };

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT productid, position 
                    FROM producthighlights 
                    ORDER BY position ASC";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int position = reader.GetInt32(1);

                        if (position >= 1 && position <= 3)
                        {
                            if (productId != 0)
                            {
                                var product = _productRepository.GetProductById(productId);
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

            return highlights;
        }


        public void RemoveHighlight(int position)
        {
            if (position < 1 || position > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(position), "Position must be between 1 and 3.");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                string query = "UPDATE producthighlights SET productid = NULL WHERE position = @Position";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Position", position);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
