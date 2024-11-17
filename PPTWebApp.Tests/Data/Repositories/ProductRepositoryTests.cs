using PPTWebApp.Data.Models;
using PPTWebApp.Data.Repositories;
using PPTWebApp.Tests.Fixtures;

namespace PPTWebApp.Tests.Data.Repositories
{
    [Collection("SharedPostgresCollection")]
    public class ProductRepositoryTests
    {
        private readonly ProductRepository _repository;

        public ProductRepositoryTests(SharedPostgresFixture fixture)
        {
            _repository = new ProductRepository(fixture.ConnectionString);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsProduct_WhenProductExists()
        {
            var testProduct = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                SKU = "TEST123",
                Price = 9.99M,
                ImageUrl = "http://example.com/image.png",
                ImageCompromise = "low"
            };

            await _repository.AddProductAsync(testProduct, CancellationToken.None);

            var insertedProduct = await _repository.GetNewestProductsAsync(null, 0, 100, 0, 1, CancellationToken.None);
            var productId = insertedProduct?.FirstOrDefault()?.Id ?? 0;

            var result = await _repository.GetProductByIdAsync(productId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal("TEST123", result.SKU);
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsNull_WhenProductDoesNotExist()
        {
            var result = await _repository.GetProductByIdAsync(99999, CancellationToken.None);

            Assert.Null(result);
        }
    }
}
