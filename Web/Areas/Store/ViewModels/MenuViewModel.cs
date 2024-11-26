using Web.Models;

namespace Web.Areas.Store.ViewModels
{
    public class MenuViewModel
    {
        public required IEnumerable<MenuItem> MenuItems { get; set; }
        public required IEnumerable<Category> Categories { get; set; }
    }
}
