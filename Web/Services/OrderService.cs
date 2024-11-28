using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order> GetOrderDetailsAsync(Guid orderId)
        {
            return await _orderRepository.GetOrderWithDetailsAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }
    }
}
