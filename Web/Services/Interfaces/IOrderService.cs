using Web.Models;
using Web.Models.Enums;

namespace Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
        Task<Order> GetOrderDetailsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<Order> GetCurrentOrderAsync(Guid userId);
        Task<(bool success, string message)> AddToCartAsync(Guid userId, Guid productId, int quantity);
        Task<(bool success, string message)> UpdateCartItemQuantityAsync(Guid userId, Guid itemId, int quantity);
        Task<(bool success, string message)> RemoveCartItemAsync(Guid userId, Guid itemId);
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    }
}
