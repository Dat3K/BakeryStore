using Microsoft.AspNetCore.Mvc;
using Web.Areas.Store.Services.Interfaces;
using Web.Models;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("Store/[controller]")]
    public class ShopController(IShopService shopService) : Controller
    {
        private readonly IShopService _shopService = shopService;

        [HttpGet]
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize, string category, string search)
        {
            ViewData["Search"] = search;
            ViewData["SelectedCategory"] = category;
            
            var result = await _shopService.GetPaginatedProductsAsync(
                pageNumber ?? 1, 
                pageSize ?? 9,
                category);
                
            return View(result);
        }

        [HttpGet]
        [Route("Products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var result = await _shopService.GetPaginatedProductsAsync(1, 9);
            return Ok(result);
        }
    }
}