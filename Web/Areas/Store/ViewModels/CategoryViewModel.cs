using Web.Models;

namespace Web.Areas.Store.ViewModels
{
    public class CategoryViewModel
    {
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? SelectedCategory { get; set; }
    }
}
