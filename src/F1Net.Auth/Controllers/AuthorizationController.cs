using System.Security.Claims;
using F1Net.Infrastructure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace F1Net.Auth.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOpenIddictApplicationManager _appManager;

    public AuthorizationController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOpenIddictApplicationManager appManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _appManager = appManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OIDC request missing.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.HasFormContentType
                        ? Request.Form.ToList()
                        : Request.Query.ToList())
                });
        }

        var user = await _userManager.GetUserAsync(result.Principal!)
            ?? throw new InvalidOperationException("User not found.");

        var identity = new ClaimsIdentity(
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user));

        identity.SetScopes(request.GetScopes());
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OIDC request missing.");

        if (request.IsClientCredentialsGrantType())
        {
            var application = await _appManager.FindByClientIdAsync(request.ClientId!)
                ?? throw new InvalidOperationException("Application not found.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, await _appManager.GetClientIdAsync(application));
            identity.SetClaim(OpenIddictConstants.Claims.Name, await _appManager.GetDisplayNameAsync(application));
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(_ => new[] { OpenIddictConstants.Destinations.AccessToken });

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var info = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return SignIn(info.Principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotSupportedException("Grant type not supported.");
    }

    [HttpGet("~/connect/userinfo"), HttpPost("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        return Ok(new
        {
            sub = await _userManager.GetUserIdAsync(user),
            email = await _userManager.GetEmailAsync(user),
            name = await _userManager.GetUserNameAsync(user),
        });
    }

    private static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
    {
        OpenIddictConstants.Claims.Name or
        OpenIddictConstants.Claims.Email or
        OpenIddictConstants.Claims.Role => new[]
        {
            OpenIddictConstants.Destinations.AccessToken,
            OpenIddictConstants.Destinations.IdentityToken
        },
        _ => new[] { OpenIddictConstants.Destinations.AccessToken }
    };
}
