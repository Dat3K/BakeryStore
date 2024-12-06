using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services.Interfaces;
using Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class POSController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public POSController(IProductService productService, IOrderService orderService, IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
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

            Console.WriteLine($"Search query: {query}");

            try
            {
                var products = await _productService.SearchProductsAsync(query, page, pageSize);
                var productData = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    sku = p.Sku,
                    thumbnail = p.Thumbnail,
                    price = p.Price,
                    stockQuantity = p.StockQuantity
                });

                return Json(new { data = productData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error searching products", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                
                var (success, message) = await _orderService.AddToCartAsync(
                    user.Sid,
                    request.ProductId,
                    request.Quantity,
                    OrderType.Pos.ToString()
                );

                if (success)
                {
                    return Json(new { success = true, message = message });
                }
                else
                {
                    return BadRequest(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error adding item to cart", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentOrder()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var order = await _orderService.GetCurrentOrderAsync(user.Sid, OrderType.Pos.ToString());
                
                if (order == null)
                {
                    return Json(new { success = false, message = "No active cart found" });
                }

                var orderData = new
                {
                    id = order.Id,
                    orderNumber = order.OrderNumber,
                    orderStatus = order.OrderStatus,
                    totalAmount = order.TotalAmount,
                    finalAmount = order.FinalAmount,
                    items = order.OrderItems.Select(item => new
                    {
                        id = item.Id,
                        productId = item.ProductId,
                        productName = item.Product.Name,
                        sku = item.Product.Sku,
                        quantity = item.Quantity,
                        unitPrice = item.UnitPrice,
                        subtotal = item.Subtotal,
                        stockQuantity = item.Product.StockQuantity,
                        thumbnail = item.Product.Thumbnail
                    })
                };

                return Json(new { success = true, data = orderData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error retrieving order", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.CheckoutAsync(user.Sid, OrderType.Pos.ToString());

                if (!success)
                    return BadRequest(new { success = false, message });

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error during checkout: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartItemQuantity([FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.UpdateCartItemQuantityAsync(
                    user.Sid,
                    request.ProductId,
                    request.Quantity,
                    OrderType.Pos.ToString()
                );

                if (success)
                {
                    return Json(new { success = true, message = message });
                }
                else
                {
                    return BadRequest(new { success = false, message = message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error updating cart item quantity", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart([FromBody] RemoveFromCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid request data" });
            }

            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.RemoveCartItemAsync(user.Sid, request.ProductId, OrderType.Pos.ToString());

                if (!success)
                {
                    return BadRequest(new { success = false, message });
                }

                return Ok(new { success = true, message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error removing item from cart: {ex.Message}" });
            }
        }

        public class AddToCartRequest
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class UpdateCartItemRequest
        {
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class RemoveFromCartRequest
        {
            [Required]
            public Guid ProductId { get; set; }
        }
    }
}
