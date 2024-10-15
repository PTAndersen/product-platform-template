using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IProductCategoryRepository
    {
        Task<IEnumerable<ProductCategory>> GetAllAsync(CancellationToken cancellationToken);
        Task<ProductCategory?> GetProductCategoryByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> AddProductCategoryAsync(ProductCategory category, CancellationToken cancellationToken);
        Task<bool> UpdateProductCategoryAsync(ProductCategory category, CancellationToken cancellationToken);
        Task<bool> DeleteProductCategoryAsync(int id, CancellationToken cancellationToken);
    }
}
