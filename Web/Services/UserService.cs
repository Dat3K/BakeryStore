using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Web.Services.DTOs;
using Web.Services.Exceptions;
using Web.Services.Interfaces;

namespace Web.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invalid user ID", nameof(id));
        }
        return await _unitOfWork.UserRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty", nameof(email));
        }
        return await _unitOfWork.UserRepository.GetByEmailAsync(email);
    }

    public async Task<User> CreateUserAsync(UserDTO userDto)
    {
        if (userDto == null)
        {
            throw new ArgumentNullException(nameof(userDto));
        }

        if (string.IsNullOrWhiteSpace(userDto.Email))
        {
            throw new ArgumentException("Email is required", nameof(userDto));
        }

        if (await _unitOfWork.UserRepository.IsEmailExistAsync(userDto.Email))
        {
            throw new BusinessException("Email already exists");
        }

        var user = new User
        {
            Sid = Guid.NewGuid(),
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Name = userDto.Name,
            Picture = userDto.Picture,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Role = Models.Enums.UserRole.Customer
        };

        await _unitOfWork.UserRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(Guid id, UserUpdateDTO userDto)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        if (userDto.FirstName != null) user.FirstName = userDto.FirstName;
        if (userDto.LastName != null) user.LastName = userDto.LastName;
        if (userDto.Name != null) user.Name = userDto.Name;
        if (userDto.Picture != null) user.Picture = userDto.Picture;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        await _unitOfWork.UserRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty", nameof(role));
        }

        return await _unitOfWork.UserRepository.GetUsersByRoleAsync((UserRole)Enum.Parse(typeof(UserRole), role));
    }
}
