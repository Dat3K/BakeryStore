using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Areas.Store.ViewModels;
using Web.Data.Repositories.Store;
using Web.Models;
using Web.Services;

namespace Web.Areas.Store.Services
{
    public class ProductService(IProductRepository productRepository) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task AddProductAsync(Product product)
        {
            await _productRepository.AddAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteProductAsync(Guid id)
        {
            await _productRepository.DeleteAsync(id);
        }

        public async Task<PaginatedResult<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize)
        {
            var products = await _productRepository.GetAllAsync();
            var totalItems = products.Count();
            var items = products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }
    }
}