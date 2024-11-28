using System;

namespace Web.Services.DTOs
{
    public class CartItemDTO
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? ProductName { get; set; }
        public string? ProductImage { get; set; }
    }
}
