using System.Collections;
using Web.Models;
using Web.ViewModels;

namespace Web.Services
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItem>> GetAllMenuItems();
        Task<MenuItem> GetMenuItemByIdAsync(Guid id);
        Task AddMenuItemAsync(MenuItem menuItem);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(Guid id);
    }
}