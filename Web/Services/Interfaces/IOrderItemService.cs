using Web.Models;

namespace Web.Services.Interfaces
{
    public interface IOrderItemService
    {
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);
        Task<OrderItem> GetOrderItemDetailsAsync(Guid orderItemId);
        Task<(bool success, string message)> UpdateQuantityAsync(Guid orderItemId, int quantity);
        Task<(bool success, string message)> DeleteOrderItemAsync(Guid orderItemId);
    }
}
