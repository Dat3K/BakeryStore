using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces;
using Web.Models;
using Web.Models.Enums;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class CartController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public CartController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
            var order = await _orderService.GetCurrentOrderAsync(user.Sid);
            return View(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckUserAddress()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var order = await _orderService.GetCurrentOrderAsync(user.Sid);
                var hasAddress = !string.IsNullOrEmpty(order?.ShippingAddress);
                return Json(new { success = true, hasAddress });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveAddress([FromBody] string address)
        {
            try
            {
                if (string.IsNullOrEmpty(address))
                {
                    return Json(new { success = false, message = "Address cannot be empty" });
                }

                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var order = await _orderService.GetCurrentOrderAsync(user.Sid);
                if (order == null)
                    return Json(new { success = false, message = "No active order found" });

                // Update order's shipping address
                order.ShippingAddress = address;
                var (success, message) = await _orderService.UpdateOrderAsync(order);
                
                return Json(new { success, message, address = order.ShippingAddress });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _orderService.GetCurrentOrderAsync(user.Sid);
            if (order == null || !order.OrderItems.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // Set initial order properties if new
            if (string.IsNullOrEmpty(order.OrderStatus))
            {
                await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Processing);
            }

            ViewBag.ShowAddressModal = string.IsNullOrEmpty(order.ShippingAddress);
            return View(order);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.AddToCartAsync(user.Sid, productId, quantity);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid itemId, int quantity)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.UpdateCartItemQuantityAsync(user.Sid, itemId, quantity);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveItem(Guid itemId)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var (success, message) = await _orderService.RemoveCartItemAsync(user.Sid, itemId);
                return Json(new { success, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync() ?? throw new Exception("User not found");
                var order = await _orderService.GetCurrentOrderAsync(user.Sid);
                var count = order?.OrderItems?.Sum(x => x.Quantity) ?? 0;
                return Json(new { success = true, count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCurrentAddress()
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var order = await _orderService.GetCurrentOrderAsync(user.Sid);
                if (order == null)
                    return Json(new { success = false, message = "No active order found" });

                return Json(new { success = true, address = order.ShippingAddress });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromForm] Order orderData)
        {
            try
            {
                var user = await _userService.GetCurrentUserAsync();
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var currentOrder = await _orderService.GetCurrentOrderAsync(user.Sid);
                if (currentOrder == null)
                    return Json(new { success = false, message = "No active order found" });

                if (string.IsNullOrEmpty(orderData.ShippingAddress))
                {
                    ModelState.AddModelError("", "Shipping address is required");
                    return View("Checkout", currentOrder);
                }

                // Update order with form data
                currentOrder.ShippingAddress = orderData.ShippingAddress;
                currentOrder.PaymentMethod = orderData.PaymentMethod;
                currentOrder.Notes = orderData.Notes;
                
                // Place the order
                var (success, message) = await _orderService.PlaceOrderAsync(currentOrder);
                
                if (success)
                {
                    return RedirectToAction("OrderConfirmation", new { orderId = currentOrder.Id });
                }
                
                ModelState.AddModelError("", message);
                return View("Checkout", currentOrder);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Checkout", orderData);
            }
        }

        [Authorize]
        public async Task<IActionResult> OrderConfirmation(Guid orderId)
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var order = await _orderService.GetOrderDetailsAsync(orderId);
            if (order == null || order.UserId != user.Sid)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }
    }
}
