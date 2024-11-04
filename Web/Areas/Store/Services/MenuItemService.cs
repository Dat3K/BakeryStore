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

        public IEnumerable<MenuItem> GetAllMenuItems()
        {
            return _menuItemRepository.GetAllAsync().Result ?? throw new Exception("No menu items found");
        }

        public MenuItem GetMenuItemById(int id)
        {
            throw new NotImplementedException();
        }

        public void AddMenuItem(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            throw new NotImplementedException();
        }

        public void DeleteMenuItem(int id)
        {
            throw new NotImplementedException();
        }
    }
}