using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Interfaces;
using Web.Services.Exceptions;

namespace Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductAsyncWithCategory();
        }

        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetProductByIdWithCategoryAsync(id) ?? throw new NotFoundException($"Product with ID {id} was not found");
            return product;
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null)
                throw new ValidationException("Product cannot be null");

            if (string.IsNullOrEmpty(product.Name))
                throw new ValidationException("Product name is required");

            if (product.Price < 0)
                throw new ValidationException("Product price cannot be negative");

            await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null)
                throw new ValidationException("Product cannot be null");

            if (string.IsNullOrEmpty(product.Name))
                throw new ValidationException("Product name is required");

            if (product.Price < 0)
                throw new ValidationException("Product price cannot be negative");

            var existingProduct = await _productRepository.GetByIdAsync(product.Id);
            if (existingProduct == null)
                throw new NotFoundException($"Product with ID {product.Id} was not found");

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new NotFoundException($"Product with ID {id} was not found");

            await _productRepository.DeleteAsync(id);
        }
    }
}
