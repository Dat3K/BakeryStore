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
            ProductSortOrder sortOrder = ProductSortOrder.Default);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
    }
}
