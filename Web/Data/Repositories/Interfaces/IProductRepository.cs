using Web.Models;

namespace Web.Data.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductAsyncWithCategory();
        Task<Product?> GetProductByIdWithCategoryAsync(Guid id);
        Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId);
    }
}
