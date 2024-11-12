using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Data.Repositories;
using Web.Data.Repositories.Store;
using Web.Models.Store;

namespace Web.Areas.Store.Repositories
{
    public class MenuItemRepository(ApplicationDbContext context) : IMenuItemRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddAsync(MenuItem menuItem)
        {
            await _context.MenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MenuItem menuItem)
        {
            _context.MenuItems.Update(menuItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MenuItem>> GetAllAsync()
        {
            return await _context.MenuItems.OrderBy(m => m.Order).ToListAsync();
        }

        public Task<MenuItem> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}