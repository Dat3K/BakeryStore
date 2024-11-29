using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Web.Services.Interfaces;

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
    }
}
