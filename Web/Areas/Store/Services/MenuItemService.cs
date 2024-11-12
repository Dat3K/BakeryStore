using System.Collections;
using Web.Data.Repositories.Store;
using Web.Models.Store;
using Web.Services;
using Web.ViewModels;

namespace Web.Areas.Store.Services
{
    public class MenuItemService(IMenuItemRepository menuItemRepository) : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository = menuItemRepository;

        public Task AddMenuItemAsync(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMenuItemAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<MenuItem>> GetAllMenuItems()
        {
            return await _menuItemRepository.GetAllAsync();
        }

        public Task<MenuItem> GetMenuItemByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }
    }
}