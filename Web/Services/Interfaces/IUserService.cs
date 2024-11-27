using Web.Models;
using Web.Services.DTOs;

namespace Web.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByNameAsync(string name);
    Task<User> CreateUserAsync(UserDTO userDto);
    Task<User> UpdateUserAsync(Guid userId, UserUpdateDTO userDto);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
}
