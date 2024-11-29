using System.Security.Authentication;
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
    private readonly IAuth0Service _auth0Service;
    private static User? _currentUser;

    public UserService(
        IUnitOfWork unitOfWork,
        IAuth0Service auth0Service)
    {
        _unitOfWork = unitOfWork;
        _auth0Service = auth0Service;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        if (_currentUser is null)
        {
            Console.WriteLine("Current user is null");
            _currentUser = await _auth0Service.GetCurrentUserAsync() ?? throw new AuthenticationException("User is null in service");
        }
        return _currentUser;
    }

    public async Task<User?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }
        return await _unitOfWork.UserRepository.GetByNameAsync(name);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _unitOfWork.UserRepository.GetUsersByRoleAsync(role);
    }
}
