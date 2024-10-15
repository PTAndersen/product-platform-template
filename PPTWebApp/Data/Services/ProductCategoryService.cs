using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PPTWebApp.Data.Services
{
    public class ProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoryService(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository ?? throw new ArgumentNullException(nameof(productCategoryRepository));
        }

        public async Task<IEnumerable<ProductCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            return await _productCategoryRepository.GetAllAsync(cancellationToken);
        }

        public async Task<ProductCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
        {
            return await _productCategoryRepository.GetProductCategoryByIdAsync(id, cancellationToken);
        }

        public async Task<int> AddCategoryAsync(ProductCategory category, CancellationToken cancellationToken)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Product category cannot be null.");
            }

            return await _productCategoryRepository.AddProductCategoryAsync(category, cancellationToken);
        }

        public async Task<bool> UpdateCategoryAsync(ProductCategory category, CancellationToken cancellationToken)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Product category cannot be null.");
            }

            return await _productCategoryRepository.UpdateProductCategoryAsync(category, cancellationToken);
        }

        public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
        {
            return await _productCategoryRepository.DeleteProductCategoryAsync(id, cancellationToken);
        }
    }
}
