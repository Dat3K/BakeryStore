using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Interfaces;
using Web.Services.Exceptions;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly DefaultdbContext _context;

        public ProductService(IProductRepository productRepository, DefaultdbContext context)
        {
            _productRepository = productRepository;
            _context = context;
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query, int page = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<Product>();

            var queryLower = $"%{query.ToLower()}%";
            return await _context.Products
                .Where(p => EF.Functions.Like(p.Name.ToLower(), queryLower) || 
                           EF.Functions.Like(p.Sku.ToLower(), queryLower) &&
                           p.IsActive == true)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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

        public async Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId)
        {
            return await _productRepository.GetProductsByCategoryAsync(categoryId);
        }

        public async Task UpdateProductPropertiesAsync(Guid productId, Dictionary<string, object> propertiesToUpdate)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                throw new NotFoundException($"Product with ID {productId} was not found");

            foreach (var property in propertiesToUpdate)
            {
                var prop = typeof(Product).GetProperty(property.Key);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(product, property.Value);
                }
            }

            await _productRepository.UpdateAsync(product);
        }
    }
}
