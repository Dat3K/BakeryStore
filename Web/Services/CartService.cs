using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Web.Services.DTOs;
using Web.Services.Interfaces;
using Web.Services.Exceptions;

namespace Web.Services;

public class CartService : ICartService
{
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID", nameof(userId));
        }
        return await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
    }

    public async Task<Cart> AddToCartAsync(Guid userId, Guid productId, int quantity)
    {
        ValidateIds(userId, productId);
        if (quantity <= 0)
        {
            throw new BusinessException("Quantity must be greater than 0");
        }

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId)
            ?? throw new NotFoundException($"Product with ID {productId} not found");

        var cart = await GetOrCreateCartAsync(userId);
        var cartItem = await _unitOfWork.CartRepository.GetCartItemAsync(cart.Id, productId);

        if (cartItem == null)
        {
            cartItem = CreateCartItem(cart.Id, product, quantity);
            cart.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
            cartItem.Subtotal = cartItem.UnitPrice * cartItem.Quantity;
        }

        UpdateCartTotals(cart);
        await _unitOfWork.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> UpdateCartItemAsync(Guid userId, Guid productId, int quantity)
    {
        ValidateIds(userId, productId);
        if (quantity < 0)
        {
            throw new BusinessException("Quantity cannot be negative");
        }

        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"Cart not found for user {userId}");

        var cartItem = await _unitOfWork.CartRepository.GetCartItemAsync(cart.Id, productId)
            ?? throw new NotFoundException($"Cart item not found for product {productId}");

        if (quantity == 0)
        {
            cart.CartItems.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = quantity;
            cartItem.Subtotal = cartItem.UnitPrice * quantity;
        }

        UpdateCartTotals(cart);
        await _unitOfWork.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        ValidateIds(userId, productId);

        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"Cart not found for user {userId}");

        var cartItem = await _unitOfWork.CartRepository.GetCartItemAsync(cart.Id, productId)
            ?? throw new NotFoundException($"Cart item not found for product {productId}");

        cart.CartItems.Remove(cartItem);
        UpdateCartTotals(cart);
        await _unitOfWork.SaveChangesAsync();
        return cart;
    }

    public async Task ClearCartAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID", nameof(userId));
        }

        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"Cart not found for user {userId}");

        cart.CartItems.Clear();
        cart.TotalAmount = 0;
        cart.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Order> CheckoutAsync(Guid userId, OrderDTO orderDto)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID", nameof(userId));
        }

        if (orderDto == null)
        {
            throw new ArgumentNullException(nameof(orderDto));
        }

        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"Cart not found for user {userId}");

        if (!cart.CartItems.Any())
        {
            throw new BusinessException("Cannot checkout with empty cart");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            TotalAmount = cart.TotalAmount,
            DiscountAmount = orderDto.DiscountAmount,
            FinalAmount = CalculateFinalAmount(cart.TotalAmount, orderDto.DiscountAmount),
            ShippingAddress = orderDto.ShippingAddress,
            Notes = orderDto.Notes,
            Status = OrderStatus.pending,
            Type = OrderType.online,
            PaymentMethod = orderDto.PaymentMethod,
            PaymentStatus = PaymentStatus.pending,
            CreatedAt = DateTime.UtcNow,
            OrderItems = CreateOrderItems(cart.CartItems, Guid.NewGuid())
        };

        await _unitOfWork.OrderRepository.AddAsync(order);
        await ClearCartAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return order;
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _unitOfWork.CartRepository.GetByUserIdAsync(userId);
        if (cart != null) return cart;

        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalAmount = 0,
            CartItems = new List<CartItem>(),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.CartRepository.AddAsync(cart);
        return cart;
    }

    private static CartItem CreateCartItem(Guid cartId, Product product, int quantity)
    {
        return new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = product.Id,
            Quantity = quantity,
            UnitPrice = product.Price,
            Subtotal = product.Price * quantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void UpdateCartTotals(Cart cart)
    {
        cart.TotalAmount = cart.CartItems.Sum(ci => ci.Subtotal);
        cart.UpdatedAt = DateTime.UtcNow;
    }

    private static List<OrderItem> CreateOrderItems(IEnumerable<CartItem> cartItems, Guid orderId)
    {
        return cartItems.Select(ci => new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = ci.ProductId,
            Quantity = ci.Quantity,
            UnitPrice = ci.UnitPrice,
            Subtotal = ci.Subtotal,
            CreatedAt = DateTime.UtcNow
        }).ToList();
    }

    private static decimal CalculateFinalAmount(decimal totalAmount, decimal? discountAmount)
    {
        return totalAmount - (discountAmount ?? 0);
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";
    }

    private static void ValidateIds(Guid userId, Guid productId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID", nameof(userId));
        }

        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Invalid product ID", nameof(productId));
        }
    }
}
