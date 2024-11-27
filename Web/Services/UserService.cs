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

    public async Task<User> CreateUserAsync(UserDTO userDto)
    {
        if (userDto == null)
        {
            throw new ArgumentNullException(nameof(userDto));
        }

        if (string.IsNullOrWhiteSpace(userDto.Name))
        {
            throw new ArgumentException("Name is required", nameof(userDto));
        }

        var user = new User
        {
            Sid = Guid.NewGuid(),
            Name = userDto.Name,
            NickName = userDto.NickName,
            Picture = userDto.Picture,
            Role = UserRole.customer,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        try 
        {
            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating user: {ex.Message}");
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(Guid userId, UserUpdateDTO userDto)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found");

        // Only allow users to update their own profile unless they are an admin
        if (currentUser?.Sid != user.Sid && currentUser?.Role != UserRole.admin)
        {
            throw new UnauthorizedException("You are not authorized to update this user's profile");
        }

        user.Name = userDto.Name ?? user.Name;
        user.NickName = userDto.NickName ?? user.NickName;
        user.Picture = userDto.Picture ?? user.Picture;
        user.UpdatedAt = DateTime.UtcNow;

        try 
        {
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating user: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var currentUser = await GetCurrentUserAsync();
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException($"User with ID {userId} not found");

        // Only allow users to delete their own account unless they are an admin
        if (currentUser?.Sid != user.Sid && currentUser?.Role != UserRole.admin)
        {
            throw new UnauthorizedException("You are not authorized to delete this user");
        }

        await _unitOfWork.UserRepository.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }
        return await _unitOfWork.UserRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role cannot be empty", nameof(role));
        }

        var currentUser = await GetCurrentUserAsync();
        if (currentUser?.Role != UserRole.admin)
        {
            throw new UnauthorizedException("Only administrators can view users by role");
        }

        return await _unitOfWork.UserRepository.GetUsersByRoleAsync((UserRole)Enum.Parse(typeof(UserRole), role));
    }
}
