using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;

namespace Web.Data.Repositories;

public class CartRepository : Repository<Cart>, ICartRepository
{
    public CartRepository(DefaultdbContext context) : base(context)
    {
    }

    public async Task<Cart?> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<IEnumerable<Cart>> GetAllWithItemsAsync()
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<Cart?> GetByIdWithItemsAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
