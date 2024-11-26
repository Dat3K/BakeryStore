using Web.Models.Enums;

namespace Web.Services.DTOs;

public class OrderDTO
{
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal? DiscountAmount { get; set; }
}
