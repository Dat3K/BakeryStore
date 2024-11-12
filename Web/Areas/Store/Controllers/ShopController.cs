using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class ShopController : Controller
    {
        
        public ShopController()
        {
        }

        public IActionResult Index()
        {
            return View();
        } 

        public IActionResult Coffee(int id)
        {
            return View();
        }
    }
}