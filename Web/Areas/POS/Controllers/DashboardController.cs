using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class DashboardController : Controller
    {
        // [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}