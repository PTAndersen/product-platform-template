using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;

namespace PPTWebApp.Data.Services
{
    public class ProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public IEnumerable<Product> GetProductsInRange(int startIndex, int range)
        {
            return _productRepository.GetProductsInRange(startIndex, range);
        }

        public int GetTotalProductCount()
        {
            return _productRepository.GetTotalProductCount();
        }

        public Product GetProductById(int id)
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
    }
}
