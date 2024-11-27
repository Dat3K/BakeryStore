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

        public async Task<Order> CreateOrderAsync(Guid userId, IEnumerable<CartItem> cartItems, string shippingAddress, string notes)
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.UnitPrice),
                ShippingAddress = shippingAddress,
                Notes = notes,
                Status = OrderStatus.pending,
                Type = OrderType.online,
                PaymentMethod = PaymentMethod.cash,
                PaymentStatus = PaymentStatus.pending,
                CreatedAt = DateTime.UtcNow
            };

            order.FinalAmount = order.TotalAmount - (order.DiscountAmount ?? 0);

            order.OrderItems = cartItems.Select(ci => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.UnitPrice,
                Subtotal = ci.Quantity * ci.UnitPrice,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _orderRepository.AddAsync(order);
            return order;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, status);
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
