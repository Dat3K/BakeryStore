using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Web.Services.Exceptions;
using Web.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Web.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuth0Service _auth0Service;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _cache;
    private const string USER_CACHE_KEY = "CurrentUser_{0}"; // {0} will be replaced with user's auth id
    private readonly TimeSpan CACHE_DURATION = TimeSpan.FromDays(180);

    public UserService(
        IUnitOfWork unitOfWork,
        IAuth0Service auth0Service,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _auth0Service = auth0Service;
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        // Check if user is authenticated
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal?.Identity?.IsAuthenticated != true)
        {
            throw new AuthenticationException("User is not authenticated");
        }

        // Get user's auth id from claims
        var authId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(authId))
        {
            throw new AuthenticationException("Auth ID not found in claims");
        }

        // Try to get user from cache
        var cacheKey = string.Format(USER_CACHE_KEY, authId);
        if (_cache.TryGetValue(cacheKey, out User? cachedUser))
        {
            return cachedUser;
        }

        // If not in cache, get from Auth0
        var user = await _auth0Service.GetCurrentUserAsync();
        if (user == null)
        {
            throw new AuthenticationException("User not found in Auth0");
        }

        // Cache the user
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CACHE_DURATION)
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                Console.WriteLine($"User cache evicted for key {key}, reason: {reason}");
            });

        _cache.Set(cacheKey, user, cacheOptions);

        return user;
    }

    public void ClearCurrentUserCache()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var authId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(authId))
        {
            var cacheKey = string.Format(USER_CACHE_KEY, authId);
            _cache.Remove(cacheKey);
        }
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
