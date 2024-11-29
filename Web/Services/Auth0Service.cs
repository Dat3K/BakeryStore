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
using System.Runtime.Serialization;

namespace Web.Services;

public interface IAuth0Service
{
    Task<User?> GetCurrentUserAsync();
    Task LoginAsync(string returnUrl = "/");
    Task LogoutAsync();
    Task<User> ProcessLoginCallbackAsync();
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

        foreach (var claim in context.User.Claims)
        {
            _logger.LogInformation($"Claim: {claim.Type} = {claim.Value}");
        }

        var nameIdentifier = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(nameIdentifier))
        {
            _logger.LogWarning("Required claims missing. NameIdentifier: {NameIdentifier}", nameIdentifier);
            return null;
        }

        var auth0User = await GetAuth0UserAsync(nameIdentifier);
        var email = auth0User.GetProperty("email").GetString();
        var name = auth0User.GetProperty("name").GetString();
        var picture = auth0User.GetProperty("picture").GetString();
        var nickname = auth0User.GetProperty("nickname").GetString();

        // Parse role from claims, defaulting to customer if not found or invalid
        var role = UserRole.Customer;
        var roleClaim = context.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value?.ToLowerInvariant();

        if (!string.IsNullOrEmpty(roleClaim))
        {
            try
            {
                // Use the same conversion logic as PostgresEnumConverter
                var enumType = typeof(UserRole);
                foreach (var nameOfEnum in Enum.GetNames(enumType))
                {
                    var memberInfo = enumType.GetMember(nameOfEnum)[0];
                    var enumMemberAttribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(EnumMemberAttribute));
                    if (enumMemberAttribute?.Value == roleClaim || nameOfEnum.ToLowerInvariant() == roleClaim)
                    {
                        role = (UserRole)Enum.Parse(enumType, nameOfEnum);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing role claim: {RoleClaim}, defaulting to customer", roleClaim);
            }
        }

        try
        {
            // Find existing user or create new one
            var user = await _unitOfWork.UserRepository.GetByNameIdentifierAsync(nameIdentifier);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Name = name,
                    Nickname = nickname,
                    Picture = picture,
                    NameIdentifier = nameIdentifier,
                    Role = role.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Created new user. Email: {Email}, Role: {Role}", email, role.ToString());
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
                if (user.Nickname != nickname)
                {
                    user.Nickname = nickname;
                    needsUpdate = true;
                }
                if (user.NameIdentifier != nameIdentifier)
                {
                    user.NameIdentifier = nameIdentifier;
                    needsUpdate = true;
                }

                if (needsUpdate)
                {
                    _logger.LogInformation($"Updating user information for email: {email}");
                    user.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.UserRepository.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation("Updated user information for email: {Email}", email);
                }
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Auth0 user information");
            throw;
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
            throw new Exception("Error when logging out", ex);
        }
    }

    public async Task<User> ProcessLoginCallbackAsync()
    {
        var user = await GetCurrentUserAsync();
        return user ?? throw new AuthenticationException("User not found");
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

        return _managementApiToken?? throw new InvalidOperationException("Failed to get management API token");
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
