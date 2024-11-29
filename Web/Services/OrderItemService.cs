using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Services.Interfaces;

namespace Web.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            return await _unitOfWork.OrderItemRepository.GetOrderItemsByOrderIdAsync(orderId);
        }

        public async Task<OrderItem> GetOrderItemDetailsAsync(Guid orderItemId)
        {
            return await _unitOfWork.OrderItemRepository.GetOrderItemWithDetailsAsync(orderItemId);
        }

        public async Task<(bool success, string message)> UpdateQuantityAsync(Guid orderItemId, int quantity)
        {
            try
            {
                var orderItem = await _unitOfWork.OrderItemRepository.GetOrderItemWithDetailsAsync(orderItemId);
                if (orderItem == null)
                    return (false, "Order item not found");

                if (quantity <= 0)
                    return (false, "Invalid quantity");

                var product = await _unitOfWork.ProductRepository.GetByIdAsync(orderItem.ProductId ?? Guid.Empty);
                if (product == null)
                    return (false, "Product not found");

                if (product.StockQuantity < quantity)
                    return (false, "Not enough stock available");

                await _unitOfWork.OrderItemRepository.UpdateOrderItemQuantityAsync(orderItemId, quantity);
                await _unitOfWork.SaveChangesAsync();

                return (true, "Quantity updated successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool success, string message)> DeleteOrderItemAsync(Guid orderItemId)
        {
            try
            {
                var orderItem = await _unitOfWork.OrderItemRepository.GetByIdAsync(orderItemId);
                if (orderItem == null)
                    return (false, "Order item not found");

                await _unitOfWork.OrderItemRepository.DeleteAsync(orderItem);
                await _unitOfWork.SaveChangesAsync();

                return (true, "Order item deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
