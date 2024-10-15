using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);

        public Task<int> GetTotalProductCountAsync(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetBestsellersAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetNewestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetOldestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetCheapestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetMostExpensiveProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task AddProductAsync(Product product, CancellationToken cancellationToken);

        Task UpdateProductAsync(Product product, CancellationToken cancellationToken);

        Task DeleteProductAsync(int id, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> GetTopDiscountedProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<IEnumerable<Product>> SearchProductsAsync(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken);

        Task<List<(int Sales, Product Product)>> GetTopSellingProductsAsync(int topProductsCount, CancellationToken cancellationToken);

    }
}
