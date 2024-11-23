using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    [Route("Store/[controller]")]
    public class ShopController(IProductService productService) : Controller
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize, string search)
        {
            ViewData["Search"] = search;
            var result = await _productService.GetPaginatedProductsAsync(pageNumber ?? 1, pageSize ?? 9);
            return View(result);
        }

        [HttpGet]
        [Route("Products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            var product = _productService.GetPaginatedProductsAsync(1, 9);
            return Ok(product);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(Product product)
        {
            await _productService.AddProductAsync(product);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            await _productService.UpdateProductAsync(product);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}