using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Web.Services.Interfaces;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IUserService _userService;

        public CartController(ICartService cartService, IUserService userService)
        {
            _cartService = cartService;
            _userService = userService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetCurrentUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var cart = await _cartService.GetCartByUserIdAsync(user.Sid);
            return View(cart);
        }
    }
}
