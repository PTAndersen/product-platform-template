using PPTWebApp.Data.Models;

namespace PPTWebApp.Data.Repositories
{
    public interface IProductInventoryRepository
    {
        ProductInventory? GetProductInventoryById(int id);
        int AddProductInventory(ProductInventory productInventory);
        bool UpdateProductInventory(ProductInventory productInventory);
        bool DeleteProductInventory(int id);
    }
}
