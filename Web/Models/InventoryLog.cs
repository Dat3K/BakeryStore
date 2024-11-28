using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class InventoryLog
{
    public Guid Id { get; set; }

    public Guid? ProductId { get; set; }

    public string Action { get; set; } = null!;

    public int ChangeQuantity { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Product? Product { get; set; }
}
