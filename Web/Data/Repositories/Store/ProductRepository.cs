using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories
{
    public class ProductRepository(DefaultdbContext context) : Repository<Product>(context), IProductRepository
    {
        public async Task<IEnumerable<Product>> GetAllProductAsyncWithCategory()
        {
            return await _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public Task<Product?> GetProductByIdWithCategoryAsync(Guid id)
        {
            return _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<List<Product>> GetProductsByCategoryAsync(Guid categoryId)
        {
            return _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive == true)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
