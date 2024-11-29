using Web.Models;

namespace Web.Data.Repositories.Interfaces
{
    public interface IOrderItemRepository : IRepository<OrderItem>
    {
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);
        Task<OrderItem> GetOrderItemWithDetailsAsync(Guid orderItemId);
        Task UpdateOrderItemQuantityAsync(Guid orderItemId, int quantity);
    }
}
