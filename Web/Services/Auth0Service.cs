using System.Security.Authentication;
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
    Task LoginAsync(string returnUrl = "/");
    Task LogoutAsync();
    Task<User> ProcessLoginCallbackAsync(AuthenticationProperties props);
}

public class Auth0Service : IAuth0Service
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Auth0Settings _auth0Settings;
    private const string PictureClaimType = "picture";

    public Auth0Service(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        Auth0Settings auth0Settings)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auth0Settings = auth0Settings ?? throw new ArgumentNullException(nameof(auth0Settings));
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            return null;

        var email = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return null;

        return await _unitOfWork.UserRepository.GetByEmailAsync(email);
    }

    public async Task LoginAsync(string returnUrl = "/")
    {
        var context = _httpContextAccessor.HttpContext ?? 
            throw new InvalidOperationException("HttpContext is not available");

        var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
            .WithRedirectUri($"/Store/Account/Callback")
            .WithParameter("returnUrl", returnUrl)
            .Build();

        await context.ChallengeAsync(
            Auth0Constants.AuthenticationScheme,
            authenticationProperties);
    }

    public async Task LogoutAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        try
        {
            // 1. Đăng xuất khỏi Auth0
            var auth0LogoutUrl = $"https://{_auth0Settings.Domain}/v2/logout?client_id={_auth0Settings.ClientId}&returnTo={context.Request.Scheme}://{context.Request.Host}";
            
            // 2. Xóa cookie xác thực của ứng dụng
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(Auth0Constants.AuthenticationScheme);

            // 3. Xóa tất cả các cookie trong domain hiện tại
            foreach (var cookie in context.Request.Cookies.Keys)
            {
                context.Response.Cookies.Delete(cookie, new CookieOptions 
                { 
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
            }

            // 4. Xóa claims và identity
            context.User = new ClaimsPrincipal(new ClaimsIdentity());

            // 5. Chuyển hướng đến trang logout của Auth0
            context.Response.Redirect(auth0LogoutUrl);
        }
        catch (Exception ex)
        {
            // Log error
            throw;
        }
    }

    public async Task<User> ProcessLoginCallbackAsync(AuthenticationProperties props)
    {
        var context = _httpContextAccessor.HttpContext ?? 
            throw new InvalidOperationException("HttpContext is not available");

        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result?.Succeeded ?? true)
            throw new AuthenticationException("Authentication failed");

        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            throw new AuthenticationException("Email claim not found");

        var user = await _unitOfWork.UserRepository.GetByEmailAsync(email);
        
        if (user == null)
        {
            user = new User
            {
                Sid = Guid.NewGuid(),
                Email = email,
                FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
                LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
                Picture = claims.FirstOrDefault(c => c.Type == PictureClaimType)?.Value,
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(user);
        }
        else
        {
            user.FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? user.FirstName;
            user.LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? user.LastName;
            user.Picture = claims.FirstOrDefault(c => c.Type == PictureClaimType)?.Value ?? user.Picture;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.UpdateAsync(user);
        }

        await _unitOfWork.SaveChangesAsync();
        return user;
    }
}
