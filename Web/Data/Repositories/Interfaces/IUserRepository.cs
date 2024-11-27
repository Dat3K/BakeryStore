using Web.Models;
using Web.Models.Enums;

namespace Web.Data.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailExistAsync(string email);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task UpdateAsync(User user);
}
