using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Web.Data.Repositories.Interfaces;
using Web.Services.Interfaces;

namespace Web.Areas.Store.Controllers
{
    [Area("Store")]
    public class CartController : Controller
    {
        public CartController()
        {
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
