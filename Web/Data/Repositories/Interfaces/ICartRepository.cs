using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web.Models;

namespace Web.Data.Repositories.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetByUserIdAsync(Guid userId);
    Task<CartItem?> GetCartItemAsync(Guid cartId, Guid productId);
    Task<IEnumerable<Cart>> GetAllWithItemsAsync();
    Task<Cart?> GetByIdWithItemsAsync(Guid id);
}
