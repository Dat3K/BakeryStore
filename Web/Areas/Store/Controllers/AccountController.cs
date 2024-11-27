using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.Enums;
using Web.Services;

namespace Web.Areas.Store.Controllers
{
    /// <summary>
    /// Controller responsible for handling user authentication and account-related actions
    /// </summary>
    [Area("Store")]
    public class AccountController : Controller
    {
        private readonly IAuth0Service _auth0Service;

        public AccountController(IAuth0Service auth0Service)
        {
            _auth0Service = auth0Service ?? throw new ArgumentNullException(nameof(auth0Service));
        }

        /// <summary>
        /// Initiates the login process using Auth0
        /// </summary>
        /// <param name="returnUrl">URL to redirect to after successful login</param>
        public async Task<IActionResult> Login(string returnUrl = "/")
        {
            try
            {
                // Check if user is already authenticated
                var currentUser = await _auth0Service.GetCurrentUserAsync();
                if (currentUser != null)
                {
                    // Redirect admin users to dashboard
                    if (currentUser.Role == UserRole.Admin)
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "POS" });
                    }
                    // Redirect regular users to home page
                    return RedirectToAction("Index", "Home", new { area = "Store" });
                }

                await _auth0Service.LoginAsync(returnUrl);
                return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "An error occurred during login." });
            }
        }

        /// <summary>
        /// Handles user logout
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _auth0Service.LogoutAsync();
                return RedirectToAction("Index", "Home", new { area = "Store" });
            }
            catch (Exception ex)
            {
                // Log the error
                return RedirectToAction("Error", "Home", new { message = "An error occurred during logout." });
            }
        }

        /// <summary>
        /// Handles the callback from Auth0 after successful authentication
        /// </summary>
        public async Task<IActionResult> Callback()
        {
            try
            {
                var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties();
                var returnUrl = props.GetParameter<string>("returnUrl") ?? "/Store";
                
                var user = await _auth0Service.ProcessLoginCallbackAsync(props);
                
                if (user.Role == UserRole.Admin)
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "POS" });
                }
                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { message = "An error occurred during authentication." });
            }
        }

        /// <summary>
        /// Displays user profile information
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            try
            {
                var user = await _auth0Service.GetCurrentUserAsync();
                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { message = "An error occurred while loading profile." });
            }
        }

        /// <summary>
        /// Handles access denied scenarios
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Gets current user by API call
        /// </summary>
        /// <returns>User object</returns>
        [Authorize]
        [HttpGet]
        [Route("api/current-user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var user = await _auth0Service.GetCurrentUserAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while getting current user.");
            }
        }

        
    }
}
