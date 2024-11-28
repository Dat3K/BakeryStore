using Web.Areas.Store.Enums;
using Web.Areas.Store.ViewModels;
using Web.Models;

namespace Web.Areas.Store.Services.Interfaces
{
    public interface IShopService
    {
        Task<PaginatedResult<Product>> GetPaginatedProductsAsync(
            int pageNumber,
            int pageSize,
            string? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            ProductSortOrder sortOrder = ProductSortOrder.Default,
            string? searchTerm = null);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Product?> GetProductByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetRelatedProductsAsync(Guid productId, int count = 4);
    }
}
