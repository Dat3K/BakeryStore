using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.POS.Controllers
{
    [Area("POS")]
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Create(Customer customer)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         // TODO: Add your customer creation logic here
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(customer);
        // }

        // public IActionResult Edit(int id)
        // {
        //     // TODO: Add your customer retrieval logic here
        //     return View();
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public IActionResult Edit(int id, Customer customer)
        // {
        //     if (id != customer.Id)
        //     {
        //         return NotFound();
        //     }

        //     if (ModelState.IsValid)
        //     {
        //         // TODO: Add your customer update logic here
        //         return RedirectToAction(nameof(Index));
        //     }
        //     return View(customer);
        // }

        // public IActionResult Delete(int id)
        // {
        //     // TODO: Add your customer deletion logic here
        //     return RedirectToAction(nameof(Index));
        // }
    }
}
