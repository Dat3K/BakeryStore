using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuth0Service _auth0Service;

    public UserService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IAuth0Service auth0Service)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _auth0Service = auth0Service;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        return await _auth0Service.GetCurrentUserAsync();
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

        // Check if user already exists
        var existingUser = await GetByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            throw new BusinessException("User with this email already exists");
        }

        var user = new User
        {
            Sid = Guid.NewGuid(),
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Picture = userDto.Picture,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Role = UserRole.Customer
        };

        await _unitOfWork.UserRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(Guid id, UserUpdateDTO userDto)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        // Only allow users to update their own profile unless they are an admin
        if (currentUser?.Sid != user.Sid && currentUser?.Role != UserRole.Admin)
        {
            throw new UnauthorizedException("You are not authorized to update this user's profile");
        }

        if (userDto.FirstName != null)
        {
            user.FirstName = userDto.FirstName;
        }
        if (userDto.LastName != null)
        {
            user.LastName = userDto.LastName;
        }
        if (userDto.Picture != null) user.Picture = userDto.Picture;

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        // Only allow users to delete their own account unless they are an admin
        if (currentUser?.Sid != user.Sid && currentUser?.Role != UserRole.Admin)
        {
            throw new UnauthorizedException("You are not authorized to delete this user");
        }

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

        var currentUser = await GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.Admin)
        {
            throw new UnauthorizedException("Only administrators can view users by role");
        }

        return await _unitOfWork.UserRepository.GetUsersByRoleAsync((UserRole)Enum.Parse(typeof(UserRole), role));
    }
}
