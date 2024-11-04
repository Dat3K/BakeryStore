using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class HomeController : Controller
    {
        private readonly IMenuItemService _menuItemService;
        
        public HomeController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        public IActionResult Index()
        {
            // Get all menu items
            var menuItems = _menuItemService.GetAllMenuItems();
            ViewBag.MenuItems = menuItems;
            return View();
        }

        // GET: Store/Home/About
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        // GET: Store/Home/Contact
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            return View();
        }
    }
}