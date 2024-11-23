using System.Linq.Expressions;

namespace Web.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<T?> GetByIdAsync(Guid id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> filter);
        IQueryable<T> GetQueryable();
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> filter, int pageNumber, int pageSize);
    }
}
