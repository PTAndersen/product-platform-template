using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;

namespace PPTWebApp.Data.Services
{
    public class ProductCategoryService
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoryService(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository ?? throw new ArgumentNullException(nameof(productCategoryRepository));
        }

        public IEnumerable<ProductCategory> GetAllCategories()
        {
            return _productCategoryRepository.GetAll();
        }

        public ProductCategory? GetCategoryById(int id)
        {
            return _productCategoryRepository.GetProductCategoryById(id);
        }

        public int AddCategory(ProductCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Product category cannot be null.");
            }

            return _productCategoryRepository.AddProductCategory(category);
        }

        public bool UpdateCategory(ProductCategory category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Product category cannot be null.");
            }

            return _productCategoryRepository.UpdateProductCategory(category);
        }

        public bool DeleteCategory(int id)
        {
            return _productCategoryRepository.DeleteProductCategory(id);
        }
    }
}
