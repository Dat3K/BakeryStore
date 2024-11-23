using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Areas.Store.ViewModels;
using Web.Models;

namespace Web.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(Guid id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid id);
        Task<PaginatedResult<Product>> GetPaginatedProductsAsync(int pageNumber, int pageSize);
    }
}