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
        {
            throw new UnauthorizedException("Only administrators can view users by role");
        }

        return await _unitOfWork.UserRepository.GetUsersByRoleAsync((UserRole)Enum.Parse(typeof(UserRole), role));
    }
}
