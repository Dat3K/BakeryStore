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

        [HttpPost]
        public IActionResult ProcessSale([FromBody] SaleRequest request)
        {
            // TODO: Implement sale processing logic
            return Json(new { success = true, message = "Sale processed successfully" });
        }

        [HttpGet]
        public IActionResult GetProducts(string query)
        {
            // TODO: Implement product search logic
            return Json(new { products = new[] { new { id = 1, name = "Sample Product", price = 10.00 } } });
        }

        [HttpGet]
        public IActionResult GetCustomers(string query)
        {
            // TODO: Implement customer search logic
            return Json(new { customers = new[] { new { id = 1, name = "Sample Customer" } } });
        }
    }

    public class SaleRequest
    {
        public int CustomerId { get; set; }
        public List<SaleItem> Items { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class SaleItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
