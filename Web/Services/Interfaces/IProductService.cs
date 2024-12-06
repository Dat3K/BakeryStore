using Web.Models;

namespace Web.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(Guid id);
        Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid id);
        Task UpdateProductPropertiesAsync(Guid productId, Dictionary<string, object> propertiesToUpdate);
    }
}
