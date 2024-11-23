using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class MenuItem
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Title { get; set; } = null!;

    public string? Url { get; set; }

    public string? Icon { get; set; }

    public int? Order { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<MenuItem> InverseParent { get; set; } = new List<MenuItem>();

    public virtual MenuItem? Parent { get; set; }
}
