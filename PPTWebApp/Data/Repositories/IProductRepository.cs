using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public interface IProductRepository
    {
        Product? GetProductById(int id);

        public int GetTotalProductCount(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice);

        IEnumerable<Product> GetBestsellers(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetNewestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetOldestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetCheapestProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetMostExpensiveProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        void AddProduct(Product product);

        void UpdateProduct(Product product);

        void DeleteProduct(int id);

        IEnumerable<Product> GetTopDiscountedProducts(ProductCategory? productCategory, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> SearchProducts(ProductCategory? productCategory, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range);
    }
}
