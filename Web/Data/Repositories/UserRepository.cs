using Microsoft.EntityFrameworkCore;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;

namespace Web.Data.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    private readonly DefaultdbContext _context;

    public UserRepository(DefaultdbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Name == name);
    }

    public async Task<bool> IsNameExistAsync(string name)
    {
        return await _context.Users
            .AnyAsync(u => u.Name == name);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(u => u.Role == role)
            .ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}
