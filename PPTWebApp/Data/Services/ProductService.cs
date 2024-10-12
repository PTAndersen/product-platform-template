using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PPTWebApp.Data.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IEnumerable<Product> GetHighlightedProducts(int range)
        {
            return _productRepository.GetBestsellers(null, 0, 100000, 0, range);
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _productRepository.GetBestsellers(null, 0, 100000, 0, GetTotalProductCount(null, "", 0, 100000));
        }

        public IEnumerable<Product> GetBestsellers(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            return _productRepository.GetBestsellers(productCategory, minPrice, maxPrice, startIndex, range);
        }

        public IEnumerable<Product> GetCheapestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            return _productRepository.GetCheapestProducts(productCategory, minPrice, maxPrice, startIndex, range);
        }

        public IEnumerable<Product> GetMostExpensiveProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            return _productRepository.GetMostExpensiveProducts(productCategory, minPrice, maxPrice, startIndex, range);
        }

        public IEnumerable<Product> SearchProducts(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range)
        {
            return _productRepository.SearchProducts(productCategory, keyword, minPrice, maxPrice, startIndex, range);
        }

        public IEnumerable<Product> GetProductsInRange(int startIndex, int range)
        {
            return _productRepository.GetBestsellers(null, 0, 100000, startIndex, range);
        }

        public int GetTotalProductCount(ProductCategory? category, string keyword, decimal minPrice, decimal maxPrice)
        {
            return _productRepository.GetTotalProductCount(category, keyword, minPrice, maxPrice);
        }

        public Product? GetProductById(int id)
        {
            return _productRepository.GetProductById(id);
        }

        public void AddProduct(Product product)
        {
            _productRepository.AddProduct(product);
        }

        public void UpdateProduct(Product product)
        {
            _productRepository.UpdateProduct(product);
        }

        public void DeleteProduct(int id)
        {
            _productRepository.DeleteProduct(id);
        }

        public async Task<List<(int Sales, Product Product)>> GetTopSellingProductsAsync(int topProductsCount)
        {
            return await _productRepository.GetTopSellingProductsAsync(topProductsCount);
        }
    }
}
