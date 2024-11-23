using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class OrderItem
{
    public Guid Id { get; set; }

    public Guid? OrderId { get; set; }

    public Guid? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
