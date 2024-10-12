using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IProductCategoryRepository
    {
        IEnumerable<ProductCategory> GetAll();
        ProductCategory? GetProductCategoryById(int id);
        int AddProductCategory(ProductCategory category);
        bool UpdateProductCategory(ProductCategory category);
        bool DeleteProductCategory(int id);
    }
}
