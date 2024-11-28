using Web.Models;
using Web.Models.Enums;
using Web.Services.DTOs;

namespace Web.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync();
    Task<User?> GetByNameAsync(string name);
    Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
}
