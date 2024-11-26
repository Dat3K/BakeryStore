using Web.Models;
using Web.Services.DTOs;

namespace Web.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateUserAsync(UserDTO userDto);
    Task<User> UpdateUserAsync(Guid id, UserUpdateDTO userDto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
}
