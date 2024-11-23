using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Data.Repositories.Store
{
    public interface IMenuItemRepository
    {
        Task<IEnumerable<MenuItem>> GetAllAsync();
        Task<MenuItem> GetByIdAsync(Guid id);
        Task<MenuItem> AddAsync(MenuItem menuItem);
        Task UpdateAsync(MenuItem menuItem);
        Task DeleteAsync(Guid id);   
    }
}