using Web.Models;
using Web.Models.Enums;

namespace Web.Data.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNameAsync(string name);
    Task<bool> IsNameExistAsync(string name);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    Task UpdateAsync(User user);
}
