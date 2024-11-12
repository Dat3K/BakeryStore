using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web.Services;

namespace Web.Areas.Store.ViewComponents
{
    public class MenuViewComponent(IMenuItemService menuItemService) : ViewComponent
    {
        private readonly IMenuItemService _menuItemService = menuItemService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _menuItemService.GetAllMenuItems();
            return View(menuItems);
        }
    }
}