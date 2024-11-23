using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories
{
    public class ProductRepository(DefaultdbContext context) : Repository<Product>(context), IProductRepository
    {
        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public override async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
