using Web.Models;

namespace Web.Data.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductAsyncWithCategory();
    }
}
