using System.Security.Authentication;
using System.Security.Claims;
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Web.Config;
using Web.Data.Repositories.Interfaces;
using Web.Models;
using Web.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Web.Services;

public interface IAuth0Service
{
    Task<User?> GetCurrentUserAsync();
    Task LoginAsync(string returnUrl = "/");
    Task LogoutAsync();
    Task<User> ProcessLoginCallbackAsync(AuthenticationProperties properties, AuthenticationTicket result);
}

public class Auth0Service : IAuth0Service
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<Auth0Settings> _auth0Settings;
    private readonly ILogger<Auth0Service> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private string? _managementApiToken;
    private DateTime _tokenExpirationTime;
    private const string PictureClaimType = "picture";

    public Auth0Service(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        IOptions<Auth0Settings> auth0Settings,
        ILogger<Auth0Service> logger,
        IHttpClientFactory httpClientFactory)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _auth0Settings = auth0Settings ?? throw new ArgumentNullException(nameof(auth0Settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            _logger.LogInformation("User is not authenticated");
            return null;
        }

        // Get the nameIdentifier (sub) claim which contains the Auth0 ID
        var nameIdentifier = context.User.Claims.FirstOrDefault(c => 
            c.Type == ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation($"Looking up user by nameIdentifier: {nameIdentifier}");

        if (string.IsNullOrEmpty(nameIdentifier))
        {
            _logger.LogWarning("NameIdentifier claim not found in token");
            return null;
        }

        try
        {
            var auth0User = await GetAuth0UserAsync(nameIdentifier);
            
            // Extract user information from Auth0
            var email = auth0User.GetProperty("email").GetString()?? throw new AuthenticationException("Email claim not found");
            var name = auth0User.GetProperty("name").GetString()?? throw new AuthenticationException("Name claim not found");
            var picture = auth0User.TryGetProperty("picture", out var pictureElement) ? pictureElement.GetString() : null;
            var nickname = auth0User.TryGetProperty("nickname", out var nicknameElement) ? nicknameElement.GetString() : null;

            // Try to find existing user by email
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    Name = name,
                    NickName = nickname,
                    Picture = picture,
                    Role = UserRole.customer,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Created new user with email: {email}");
            }
            else
            {
                // Update existing user if needed
                bool needsUpdate = false;

                if (user.Name != name)
                {
                    user.Name = name;
                    needsUpdate = true;
                }
                if (user.Picture != picture)
                {
                    user.Picture = picture;
                    needsUpdate = true;
                }
                if (user.NickName != nickname)
                {
                    user.NickName = nickname;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.UserRepository.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated user information for email: {email}");
                }
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Auth0 user information");
            return null;
        }
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
            var auth0LogoutUrl = $"https://{_auth0Settings.Value.Domain}/v2/logout?client_id={_auth0Settings.Value.ClientId}&returnTo={context.Request.Scheme}://{context.Request.Host}";
            
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

    public async Task<User> ProcessLoginCallbackAsync(AuthenticationProperties properties, AuthenticationTicket result)
    {
        var claims = result.Principal.Claims;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            throw new AuthenticationException("Email claim not found");

        if (string.IsNullOrEmpty(name))
            throw new AuthenticationException("Name claim not found");

        // Try to find user by email first
        var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            // Get the nameIdentifier (sub) claim which contains the Auth0 ID
            var nameIdentifier = claims.FirstOrDefault(c => 
                c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Create new user
            user = new User
            {
                Sid = Guid.NewGuid(),
                Email = email,
                Name = name,
                NickName = claims.FirstOrDefault(c => c.Type == "nickname")?.Value,
                Picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Role = UserRole.customer // Default role for new users
            };

            // Get email provider if available
            if (!string.IsNullOrEmpty(nameIdentifier))
            {
                try
                {
                    var provider = await GetUserEmailProviderAsync(nameIdentifier);
                    _logger.LogInformation($"User {email} authenticated with provider: {provider}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get email provider for user {Email}", email);
                }
            }

            await _unitOfWork.UserRepository.AddAsync(user);
        }
        else
        {
            // Update existing user information
            bool needsUpdate = false;
            
            var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;
            var nickname = claims.FirstOrDefault(c => c.Type == "nickname")?.Value;
            
            if (user.Name != name)
            {
                user.Name = name;
                needsUpdate = true;
            }
            if (user.Picture != picture)
            {
                user.Picture = picture;
                needsUpdate = true;
            }
            if (user.NickName != nickname)
            {
                user.NickName = nickname;
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.UserRepository.UpdateAsync(user);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    private async Task<JsonElement> GetAuth0UserAsync(string userId)
    {
        try
        {
            var token = await GetManagementApiTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(
                $"https://{_auth0Settings.Value.Domain}/api/v2/users/{userId}"
            );

            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<JsonElement>();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Auth0 user {UserId}", userId);
            throw;
        }
    }

    private async Task<string> GetManagementApiTokenAsync()
    {
        if (_managementApiToken != null && DateTime.UtcNow < _tokenExpirationTime)
        {
            return _managementApiToken;
        }

        var client = _httpClientFactory.CreateClient();
        var tokenRequest = new
        {
            client_id = _auth0Settings.Value.ManagementApiClientId,
            client_secret = _auth0Settings.Value.ManagementApiClientSecret,
            audience = $"https://{_auth0Settings.Value.Domain}/api/v2/",
            grant_type = "client_credentials"
        };

        var response = await client.PostAsJsonAsync(
            $"https://{_auth0Settings.Value.Domain}/oauth/token",
            tokenRequest
        );

        response.EnsureSuccessStatusCode();
        var tokenResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        _managementApiToken = tokenResponse.GetProperty("access_token").GetString();
        _tokenExpirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.GetProperty("expires_in").GetInt32() - 60);

        return _managementApiToken;
    }

    public async Task<string?> GetUserEmailProviderAsync(string userId)
    {
        try
        {
            var token = await GetManagementApiTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(
                $"https://{_auth0Settings.Value.Domain}/api/v2/users/{userId}"
            );

            response.EnsureSuccessStatusCode();
            var user = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (user.TryGetProperty("identities", out var identities) && 
                identities.GetArrayLength() > 0)
            {
                var identity = identities[0];
                if (identity.TryGetProperty("provider", out var provider))
                {
                    return provider.GetString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user email provider for user {UserId}", userId);
            throw;
        }
    }
}
