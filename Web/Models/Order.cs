using System;
using System.Collections.Generic;
using Web.Models.Enums;

namespace Web.Models;

public partial class Order
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public string? ShippingAddress { get; set; }

    public string? Notes { get; set; }

    public OrderStatus Status { get; set; }

    public OrderType Type { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
