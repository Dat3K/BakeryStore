using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;

namespace Web.Areas.Store.ViewComponents
{
    public class MenuViewComponent(IMenuItemService menuItemService) : ViewComponent
    {
        private readonly IMenuItemService _menuItemService = menuItemService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _menuItemService.GetAllMenuItemsAsync();
            return View(menuItems.OrderBy(m => m.Order).ToList());
        }
    }
}