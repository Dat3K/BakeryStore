using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;
using Web.Areas.Store.ViewModels;

namespace Web.Areas.Store.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuItemService _menuItemService;
        private readonly ICategoryService _categoryService;

        public MenuViewComponent(IMenuItemService menuItemService, ICategoryService categoryService)
        {
            _menuItemService = menuItemService;
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var menuItems = await _menuItemService.GetAllMenuItemsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            var viewModel = new MenuViewModel
            {
                MenuItems = menuItems.OrderBy(m => m.Order).ToList(),
                Categories = categories.OrderBy(c => c.Name).ToList()
            };

            return View(viewModel);
        }
    }
}