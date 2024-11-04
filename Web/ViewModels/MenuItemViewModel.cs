using System;
using System.Collections.Generic;

namespace Web.ViewModels
{
    public partial class MenuItemViewModel
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string Title { get; set; } = null!;

        public string? Url { get; set; }

        public string? Icon { get; set; }

        public int? Order { get; set; }

        public bool? IsActive { get; set; }

        public virtual ICollection<MenuItemViewModel> InverseParent { get; set; } = new List<MenuItemViewModel>();

        public virtual MenuItemViewModel? Parent { get; set; }
    }
}