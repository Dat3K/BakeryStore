using Web.Models;
using Web.Models.Enums;

namespace Web.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId);
        Task<Order> GetOrderDetailsAsync(Guid orderId);
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
    }
}
