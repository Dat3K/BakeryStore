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
        private readonly IOrderService _orderService;
    

        public CustomerController(IUserService userService, IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
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
                var customers = users.Select(u => new
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

        [HttpGet]
        [Route("POS/[controller]/GetCustomerOrders/{userId}")]
        public async Task<IActionResult> GetCustomerOrders(Guid userId)
        {
            try
            {
                var orders = await _orderService.GetUserOrdersAsync(userId);
                
                // Check if orders exist
                if (!orders.Any())
                {
                    return Json(new { success = true, data = new List<object>() });
                }

                var orderData = orders.Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    Status = o.OrderStatus?.ToString(),
                    o.TotalAmount,
                    PaymentMethod = o.PaymentMethod?.ToString(),
                    CreatedAt = o.CreatedAt?.ToString("MM/dd/yyyy HH:mm:ss"),
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        oi.Id,
                        product = new
                        {
                            oi.ProductId,
                            ProductName = oi.Product?.Name,
                            ProductDescription = oi.Product?.Description,
                            ProductThumbnail = oi.Product?.Thumbnail,
                            ProductSku = oi.Product?.Sku,
                        },
                        oi.Subtotal,
                        oi.UnitPrice,
                        oi.Quantity,
                    })
                }).ToList();

                return Json(new { success = true, data = orderData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}
