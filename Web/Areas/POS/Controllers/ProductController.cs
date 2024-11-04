using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class POSController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        //     [Authorize]
        //     public IActionResult Product()
        //     {
        //         return View();
        //     }

        //     [Authorize]
        //     public IActionResult AddProduct()
        //     {
        //         return View();
        //     }
    }
}