using System.Collections;
using Web.Data.Repositories.Store;
using Web.Models;
using Web.Services;
using Web.ViewModels;

namespace Web.Areas.Store.Services
{
    public class MenuItemService(IMenuItemRepository menuItemRepository) : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository = menuItemRepository;

        public Task AddMenuItemAsync(MenuItem menuItem)
        {
            _menuItemRepository.AddAsync(menuItem);
            return Task.CompletedTask;
        }

        public Task DeleteMenuItemAsync(Guid id)
        {
            _menuItemRepository.DeleteAsync(id);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItems()
        {
            return await _menuItemRepository.GetAllAsync();
        }

        public Task<MenuItem> GetMenuItemByIdAsync(Guid id)
        {
            return _menuItemRepository.GetByIdAsync(id);
        }

        public Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            _menuItemRepository.UpdateAsync(menuItem);
            return Task.CompletedTask;
        }
    }
}