using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuth0Service _auth0Service;

    public AuthController(IAuth0Service auth0Service)
    {
        _auth0Service = auth0Service;
    }

    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        await _auth0Service.LoginAsync();
        return Ok(new { });
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _auth0Service.LogoutAsync();
        return Ok();
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback()
    {
        var props = new AuthenticationProperties { RedirectUri = "/" };
        var user = await _auth0Service.ProcessLoginCallbackAsync(props);
        return Ok(user);
    }

    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _auth0Service.GetCurrentUserAsync();
        if (user == null)
            return NotFound();
            
        return Ok(user);
    }
}
