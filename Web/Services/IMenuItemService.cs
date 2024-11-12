using System.Collections;
using Web.Models.Store;
using Web.ViewModels;

namespace Web.Services
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItem>> GetAllMenuItems();
        Task<MenuItem> GetMenuItemByIdAsync(int id);
        Task AddMenuItemAsync(MenuItem menuItem);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(int id);
    }
}