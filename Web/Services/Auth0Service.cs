using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.Config;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;

namespace Web.Services;

public interface IAuth0Service
{
    Task<User?> GetCurrentUserAsync();
    Task<string> LoginAsync();
    Task LogoutAsync();
    Task<User> ProcessLoginCallbackAsync(AuthenticationProperties props);
}

public class Auth0Service : IAuth0Service
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Auth0Settings _auth0Settings;

    public Auth0Service(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        Auth0Settings auth0Settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _auth0Settings = auth0Settings;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return null;

        return await _unitOfWork.UserRepository.GetByEmailAsync(email);
    }

    public async Task<string> LoginAsync()
    {
        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri("/callback")
            .Build();

        await _httpContextAccessor.HttpContext!.ChallengeAsync(
            Auth0Constants.AuthenticationScheme,
            authenticationProperties);

        return "/callback";
    }

    public async Task LogoutAsync()
    {
        var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
            .WithRedirectUri("/")
            .Build();

        await _httpContextAccessor.HttpContext!.SignOutAsync(
            Auth0Constants.AuthenticationScheme,
            authenticationProperties);
        await _httpContextAccessor.HttpContext!.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<User> ProcessLoginCallbackAsync(AuthenticationProperties props)
    {
        var result = await _httpContextAccessor.HttpContext!.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            throw new Exception("Authentication failed");

        var email = result.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new Exception("Email claim not found");

        var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        if (user == null)
        {
            // Create new user
            user = new User
            {
                Sid = Guid.NewGuid(),
                Email = email,
                FirstName = result.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
                LastName = result.Principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        return user;
    }
}
