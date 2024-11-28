using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string OrderStatus { get; set; } = null!;

    public string OrderType { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
