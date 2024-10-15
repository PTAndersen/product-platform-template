using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories.Interfaces
{
    public interface IProductInventoryRepository
    {
        Task<ProductInventory?> GetProductInventoryByIdAsync(int id, CancellationToken cancellationToken);
        Task<int> AddProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken);
        Task<bool> UpdateProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken);
        Task<bool> DeleteProductInventoryAsync(int id, CancellationToken cancellationToken);
    }
}
