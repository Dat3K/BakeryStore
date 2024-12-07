using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
