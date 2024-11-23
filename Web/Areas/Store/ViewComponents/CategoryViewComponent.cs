using Microsoft.AspNetCore.Mvc;
using Web.Areas.Store.Services.Interfaces;
using Web.Areas.Store.ViewModels;

namespace Web.Areas.Store.ViewComponents
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly IShopService _shopService;

        public CategoryViewComponent(IShopService shopService)
        {
            _shopService = shopService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _shopService.GetAllCategoriesAsync();
            var selectedCategory = HttpContext.Request.Query["category"].ToString();

            var viewModel = new CategoryViewModel
            {
                Categories = categories,
                SelectedCategory = selectedCategory
            };

            return View(viewModel);
        }
    }
}
