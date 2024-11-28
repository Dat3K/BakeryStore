using System;
using System.Collections.Generic;

namespace Web.Services.DTOs
{
    public class CartDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CartItemDTO> Items { get; set; } = new();
    }
}
