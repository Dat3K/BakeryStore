using Web.Areas.Store.ViewModels;
using Web.Areas.Store.Services.Interfaces;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using System.Linq;
using Web.Areas.Store.Enums;

namespace Web.Areas.Store.Services
{
    public class ShopService(IProductRepository productRepository, ICategoryRepository categoryRepository) : IShopService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        public async Task<PaginatedResult<Product>> GetPaginatedProductsAsync(
            int pageNumber, 
            int pageSize, 
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            ProductSortOrder sortOrder = ProductSortOrder.Default)
        {
            // Get all products with categories included
            var products = (await _productRepository.GetAllProductAsyncWithCategory()).ToList();
            
            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                products = products
                    .Where(p => p.Category != null && 
                           p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filter by price range if specified
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value).ToList();
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value).ToList();
            }

            // Apply sorting
            products = sortOrder switch
            {
                ProductSortOrder.NameAsc => products.OrderBy(p => p.Name).ToList(),
                ProductSortOrder.NameDesc => products.OrderByDescending(p => p.Name).ToList(),
                ProductSortOrder.PriceAsc => products.OrderBy(p => p.Price).ToList(),
                ProductSortOrder.PriceDesc => products.OrderByDescending(p => p.Price).ToList(),
                ProductSortOrder.Newest => products.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => products.OrderBy(p => p.Name).ToList() // Default sorting
            };

            // Calculate pagination
            var totalItems = products.Count;
            var items = products
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
