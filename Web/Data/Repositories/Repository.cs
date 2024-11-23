using Microsoft.EntityFrameworkCore;
using Web.Models;

namespace Web.Data.Repositories
{
    public class Repository<T>(DefaultdbContext context) : IRepository<T> where T : class
    {
        private readonly DefaultdbContext _context = context;
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T> AddAsync(T entity)
        {
            var result = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public Task DeleteAsync(Guid id)
        {
            _dbSet.Remove(_dbSet.Find(id)?? throw new InvalidOperationException("Entity not found"));
            return _context.SaveChangesAsync();
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return _context.SaveChangesAsync();
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_dbSet.AsEnumerable());
        }

        public Task<T> GetByIdAsync(Guid id)
        {
            return Task.FromResult(_dbSet.Find(id)?? throw new InvalidOperationException("Entity not found"));
        }

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return _context.SaveChangesAsync();
        }
    }
}
