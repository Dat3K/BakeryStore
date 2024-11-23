using Web.Areas.Store.ViewModels;
using Web.Areas.Store.Services.Interfaces;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Areas.Store.Services
{
    public class ShopService(IProductRepository productRepository, ICategoryRepository categoryRepository) : IShopService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        public async Task<PaginatedResult<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize, string? category = null)
        {
            var query = await _productRepository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category != null && 
                    p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = query.Count();
            var items = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.OrderBy(c => c.Name);
        }
    }
}
