using System;
using System.Collections.Generic;

namespace Web.Models;

public partial class Product
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public decimal Price { get; set; }

    public decimal CostPrice { get; set; }

    public int StockQuantity { get; set; }

    public bool? IsActive { get; set; }

    public string? Thumbnail { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<InventoryLog> InventoryLogs { get; set; } = new List<InventoryLog>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
