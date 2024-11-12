using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}