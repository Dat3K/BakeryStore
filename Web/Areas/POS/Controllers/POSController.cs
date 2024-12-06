using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services.Interfaces;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class POSController : Controller
    {
        private readonly IProductService _productService;

        public POSController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { data = Array.Empty<object>() });
            }

            try
            {
                var products = await _productService.SearchProductsAsync(query, page, pageSize);
                var productData = products.Select(p => new
                {
                    _id = p.Id,
                    name = p.Name,
                    barcode = p.Sku,
                    image = p.Thumbnail,
                    retailPrice = p.Price,
                    stockQuantity = p.StockQuantity
                });

                return Json(new { data = productData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error searching products", details = ex.Message });
            }
        }
    }
}
