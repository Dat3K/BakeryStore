using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;
using Web.Models;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [Authorize]
        public async Task<IActionResult> Create(Guid id)
        {
            return View();
        }
    }
}