using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class ProductImage
{
    public Guid Id { get; set; }

    public Guid? ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public int? DisplayOrder { get; set; }

    public bool? IsPrimary { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product? Product { get; set; }
}
