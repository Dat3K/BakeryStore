using Web.Models;

namespace Web.Services.Interfaces
{
    public interface IMenuItemService
    {
        Task<IEnumerable<MenuItem>> GetAllMenuItemsAsync();
        Task<MenuItem> GetMenuItemByIdAsync(Guid id);
        Task AddMenuItemAsync(MenuItem menuItem);
        Task UpdateMenuItemAsync(MenuItem menuItem);
        Task DeleteMenuItemAsync(Guid id);
    }
}
