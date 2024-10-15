using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetHighlightedProductsAsync(int range, CancellationToken cancellationToken)
        {
            return await _productRepository.GetBestsellersAsync(null, 0, 100000, 0, range, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(CancellationToken cancellationToken)
        {
            int totalCount = await GetTotalProductCountAsync(null, "", 0, 100000, cancellationToken);
            return await _productRepository.GetBestsellersAsync(null, 0, 100000, 0, totalCount, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetBestsellersAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _productRepository.GetBestsellersAsync(productCategory, minPrice, maxPrice, startIndex, range, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetCheapestProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _productRepository.GetCheapestProductsAsync(productCategory, minPrice, maxPrice, startIndex, range, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetMostExpensiveProductsAsync(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _productRepository.GetMostExpensiveProductsAsync(productCategory, minPrice, maxPrice, startIndex, range, cancellationToken);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _productRepository.SearchProductsAsync(productCategory, keyword, minPrice, maxPrice, startIndex, range, cancellationToken);
        }

        public async Task<IEnumerable<Product>> GetProductsInRangeAsync(int startIndex, int range, CancellationToken cancellationToken)
        {
            return await _productRepository.GetBestsellersAsync(null, 0, 100000, startIndex, range, cancellationToken);
        }

        public async Task<int> GetTotalProductCountAsync(ProductCategory? category, string keyword, decimal minPrice, decimal maxPrice, CancellationToken cancellationToken)
        {
            return await _productRepository.GetTotalProductCountAsync(category, keyword, minPrice, maxPrice, cancellationToken);
        }

        public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _productRepository.GetProductByIdAsync(id, cancellationToken);
        }

        public async Task AddProductAsync(Product product, CancellationToken cancellationToken)
        {
            await _productRepository.AddProductAsync(product, cancellationToken);
        }

        public async Task UpdateProductAsync(Product product, CancellationToken cancellationToken)
        {
            await _productRepository.UpdateProductAsync(product, cancellationToken);
        }

        public async Task DeleteProductAsync(int id, CancellationToken cancellationToken)
        {
            await _productRepository.DeleteProductAsync(id, cancellationToken);
        }

        public async Task<List<(int Sales, Product Product)>> GetTopSellingProductsAsync(int topProductsCount, CancellationToken cancellationToken)
        {
            return await _productRepository.GetTopSellingProductsAsync(topProductsCount, cancellationToken);
        }
    }
}
