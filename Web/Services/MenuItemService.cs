using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Exceptions;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class MenuItemService(IMenuItemRepository menuItemRepository) : IMenuItemService
    {
        private readonly IMenuItemRepository _menuItemRepository = menuItemRepository;

        public async Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync()
        {
            return await _menuItemRepository.GetAllAsync();
        }

        public async Task<MenuItem> GetMenuItemByIdAsync(Guid id)
        {
            var menuItem = await _menuItemRepository.GetByIdAsync(id) ?? throw new NotFoundException($"MenuItem with ID {id} was not found");
            return menuItem;
        }

        public async Task AddMenuItemAsync(MenuItem menuItem)
        {
            await _menuItemRepository.AddAsync(menuItem);
        }

        public async Task UpdateMenuItemAsync(MenuItem menuItem)
        {
            await _menuItemRepository.UpdateAsync(menuItem);
        }

        public async Task DeleteMenuItemAsync(Guid id)
        {
            await _menuItemRepository.DeleteAsync(id);
        }
    }
}
