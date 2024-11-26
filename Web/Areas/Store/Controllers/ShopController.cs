using Microsoft.AspNetCore.Mvc;
using Web.Areas.Store.Enums;
using Web.Areas.Store.Services.Interfaces;
using Web.Models;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("Store/[controller]")]
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? pageNumber, 
            int? pageSize, 
            string? category,
            decimal? minPrice,
            decimal? maxPrice,
            ProductSortOrder sortOrder = ProductSortOrder.Default,
            string? searchTerm = null)
        {
            ViewData["SelectedCategory"] = category;
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["SearchTerm"] = searchTerm;
            var result = await _shopService.GetPaginatedProductsAsync(
                pageNumber ?? 1,
                pageSize ?? 9,
                category,
                minPrice,
                maxPrice,
                sortOrder,
                searchTerm);
            return View(result);
        }
    }
}