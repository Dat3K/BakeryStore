using Web.Models;
using Web.Models.Enums;

namespace Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
        Task<Order> GetOrderDetailsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<Order> GetCurrentOrderAsync(Guid userId, string orderType = "Online");
        Task<(bool success, string message)> AddToCartAsync(Guid userId, Guid productId, int quantity, string orderType = "Online");
        Task<(bool success, string message)> UpdateCartItemQuantityAsync(Guid userId, Guid itemId, int quantity, string orderType = "Online");
        Task<(bool success, string message)> RemoveCartItemAsync(Guid userId, Guid itemId, string orderType = "Online");
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<(bool success, string message)> CheckoutAsync(Guid userId, string orderType = "Online");
    }
}
