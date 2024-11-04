using System.Collections;
using Web.Models.Store;
using Web.ViewModels;

namespace Web.Services
{
    public interface IMenuItemService
    {
        IEnumerable<MenuItem> GetAllMenuItems();
        MenuItem GetMenuItemById(int id);
        void AddMenuItem(MenuItem menuItem);
        void UpdateMenuItem(MenuItem menuItem);
        void DeleteMenuItem(int id);
    }
}