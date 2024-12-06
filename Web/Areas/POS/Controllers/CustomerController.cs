using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly IUserService _userService;

        public CustomerController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Route("POS/[controller]/GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var users = await _userService.GetAllCustomersAsync();
                var customers = users.Where(u => u.Role != "Admin")
                    .Select(u => new
                    {
                        u.Sid,
                        u.Name,
                        u.Email,
                        u.Nickname,
                        u.Picture,
                        CreatedAt = u.CreatedAt?.ToString("MM/dd/yyyy HH:mm:ss"),
                        UpdatedAt = u.UpdatedAt?.ToString("MM/dd/yyyy HH:mm:ss")
                    });

                return Json(new { data = customers });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
