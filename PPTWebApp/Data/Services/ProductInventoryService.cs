using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;

namespace PPTWebApp.Data.Services
{
    public class ProductInventoryService
    {
        private readonly IProductInventoryRepository _productInventoryRepository;

        public ProductInventoryService(IProductInventoryRepository productInventoryRepository)
        {
            _productInventoryRepository = productInventoryRepository;
        }
        public async Task<ProductInventory?> GetProductInventoryByIdAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _productInventoryRepository.GetProductInventoryByIdAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductInventoryService.GetProductInventoryByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> AddProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            if (productInventory == null)
                throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");

            try
            {
                return await _productInventoryRepository.AddProductInventoryAsync(productInventory, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductInventoryService.AddProductInventoryAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProductInventoryAsync(ProductInventory productInventory, CancellationToken cancellationToken)
        {
            if (productInventory == null)
                throw new ArgumentNullException(nameof(productInventory), "Product inventory cannot be null.");

            try
            {
                return await _productInventoryRepository.UpdateProductInventoryAsync(productInventory, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductInventoryService.UpdateProductInventoryAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteProductInventoryAsync(int id, CancellationToken cancellationToken)
        {
            try
            {
                return await _productInventoryRepository.DeleteProductInventoryAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ProductInventoryService.DeleteProductInventoryAsync: {ex.Message}");
                throw;
            }
        }
    }
}
