using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId)
        {
            return await _unitOfWork.OrderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
        {
            return await _unitOfWork.OrderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order> GetOrderDetailsAsync(Guid orderId)
        {
            return await _unitOfWork.OrderRepository.GetOrderWithDetailsAsync(orderId);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            await _unitOfWork.OrderRepository.UpdateOrderStatusAsync(orderId, status);
            await _unitOfWork.SaveChangesAsync();
        }

        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        public async Task<Order> GetCurrentOrderAsync(Guid userId, string orderType = "Online")
        {
            var order = await _unitOfWork.OrderRepository.GetCurrentOrderAsync(userId, orderType);
            
            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    OrderStatus = "Pending",
                    OrderType = orderType,
                    CreatedAt = DateTime.UtcNow,
                    OrderNumber = GenerateOrderNumber(),
                    PaymentStatus = "Pending",
                    TotalAmount = 0,
                    FinalAmount = 0
                };
                await _unitOfWork.OrderRepository.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();
            }
            
            return order;            
        }

        public async Task<(bool success, string message)> AddToCartAsync(Guid userId, Guid productId, int quantity, string orderType = "Online")
        {
            try
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                
                if (product == null)
                    return (false, "Product not found");

                if (quantity <= 0)
                    return (false, "Invalid quantity");

                if (product.StockQuantity < quantity)
                    return (false, "Not enough stock available");

                var order = await _unitOfWork.OrderRepository.GetCurrentOrderAsync(userId, orderType);
                if (order == null)
                {
                    order = new Order
                    {
                        UserId = userId,
                        OrderStatus = OrderStatus.Pending.ToString(),
                        OrderType = orderType,
                        CreatedAt = DateTime.UtcNow,
                        OrderNumber = GenerateOrderNumber(),
                        PaymentStatus = PaymentStatus.Pending.ToString(),
                        TotalAmount = 0,
                        FinalAmount = 0
                    };
                    await _unitOfWork.OrderRepository.AddAsync(order);
                }

                var orderItem = order.OrderItems.FirstOrDefault(item => item.ProductId == productId);
                
                if (orderItem == null)
                {
                    orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        Subtotal = product.Price * quantity
                    };
                    order.OrderItems.Add(orderItem);
                }
                else
                {
                    orderItem.Quantity += quantity;
                    orderItem.Subtotal += product.Price * quantity;
                }

                order.TotalAmount += product.Price * quantity;
                order.FinalAmount += product.Price * quantity;

                await _unitOfWork.SaveChangesAsync();
                return (true, "Item added to cart successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity, string orderType = "Online")
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetCurrentOrderAsync(userId, orderType);
                
                if (order == null)
                    return (false, "Order not found");

                var orderItem = order.OrderItems.FirstOrDefault(item => item.ProductId == productId);
                
                if (orderItem == null)
                    return (false, "Item not found in cart");

                if (quantity <= 0)
                {
                    order.OrderItems.Remove(orderItem);
                }
                else
                {
                    var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
                    if (product == null)
                        return (false, "Product not found");

                    if (product.StockQuantity < quantity)
                        return (false, "Not enough stock available");

                    // Calculate the price difference
                    var oldSubtotal = orderItem.Subtotal;
                    orderItem.Quantity = quantity;
                    orderItem.Subtotal = product.Price * quantity;

                    // Update order totals
                    order.TotalAmount = order.TotalAmount - oldSubtotal + orderItem.Subtotal;
                    order.FinalAmount = order.TotalAmount;
                }

                await _unitOfWork.SaveChangesAsync();
                return (true, "Cart updated successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> RemoveCartItemAsync(Guid userId, Guid itemId, string orderType = "Online")
        {
            try
            {
                var order = await _unitOfWork.OrderRepository.GetCurrentOrderAsync(userId, orderType);
                var orderItems = await _unitOfWork.OrderItemRepository.GetOrderItemsByOrderIdAsync(order.Id);
                if (order == null)
                    return (false, "Order not found");

                var orderItem = orderItems.FirstOrDefault(item => item.Id == itemId);
                
                if (orderItem == null)
                    return (false, "Item not found");

                order.OrderItems.Remove(orderItem);
                
                if (order.OrderItems.Count == 0)
                {
                    await _unitOfWork.OrderRepository.DeleteAsync(order.Id);
                }
                await _unitOfWork.OrderItemRepository.DeleteAsync(orderItem.Id);
                await _unitOfWork.SaveChangesAsync();
                return (true, "Item removed successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }


}
