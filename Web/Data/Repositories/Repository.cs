using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Web.Models;

namespace Web.Data.Repositories
{
    public class Repository<T>(DefaultdbContext context) : IRepository<T> where T : class
    {
        private readonly DefaultdbContext _context = context;
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.Where(filter).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var result = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return result.Entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id) 
                ?? throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with ID {id} not found.");
            await DeleteAsync(entity);
        }

        public virtual async Task DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await _dbSet.FindAsync(id) != null;
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.AnyAsync(filter);
        }

        public virtual async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.CountAsync(filter);
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            return await _dbSet.FirstOrDefaultAsync(filter);
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize)
        {
            return await _dbSet
                .Where(filter)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
