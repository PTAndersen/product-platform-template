using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public interface IProductRepository
    {
        Product GetProductById(int id);

        public int GetTotalProductCount(string categoryName);

        IEnumerable<Product> GetProductsByCategory(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetBestsellers(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetNewestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetOldestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetCheapestProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetMostExpensiveProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetPhysicalProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetSoftwareProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        void AddProduct(Product product);

        void UpdateProduct(Product product);

        void DeleteProduct(int id);

        IEnumerable<Product> GetTopDiscountedProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> GetInStockProducts(string categoryName, decimal minPrice, decimal maxPrice, int startIndex, int range);

        IEnumerable<Product> SearchProducts(string categoryName, string keyword, decimal minPrice, decimal maxPrice, int startIndex, int range);
    }
}
