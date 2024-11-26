using Web.Models;
using Web.Models.Enums;

namespace Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
        Task<Order> GetOrderDetailsAsync(Guid orderId);
        Task<Order> CreateOrderAsync(Guid userId, IEnumerable<CartItem> cartItems, string shippingAddress, string notes);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    }
}
