using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class POSController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
