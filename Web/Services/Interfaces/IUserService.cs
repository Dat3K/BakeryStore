using Web.Models;
using Web.Models.Enums;

namespace Web.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync();
    Task<User?> GetByNameAsync(string name);
    Task<IEnumerable<User>> GetAllCustomersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
}
