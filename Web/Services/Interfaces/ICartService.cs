using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Models;
using Web.Services.DTOs;

namespace Web.Services.Interfaces;

public interface ICartService
{
    Task<Cart?> GetCartByUserIdAsync(Guid userId);
    Task<Cart> AddToCartAsync(Guid userId, Guid productId, int quantity);
    Task<Cart> UpdateCartItemAsync(Guid userId, Guid productId, int quantity);
    Task<Cart> RemoveFromCartAsync(Guid userId, Guid productId);
    Task ClearCartAsync(Guid userId);
    Task<Order> CheckoutAsync(Guid userId, OrderDTO orderDto);
}
