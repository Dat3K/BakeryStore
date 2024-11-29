using Web.Models;
using Web.Models.Enums;

namespace Web.Data.Repositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<Order> GetOrderWithDetailsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<Order> GetCurrentOrderAsync(Guid userId);
    }
}
