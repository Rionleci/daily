using System.Security.Claims;
using Daily.Models;
using Daily.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Daily.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AuthSettings _auth;

    public AccountController(IOptions<AuthSettings> auth) => _auth = auth.Value;

    [HttpGet("/Account/Login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("/Account/Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(model);

        var validUser = string.Equals(model.Username.Trim(), _auth.Username, StringComparison.Ordinal);
        var validPass = string.Equals(model.Password, _auth.Password, StringComparison.Ordinal);

        if (!validUser || !validPass)
        {
            ModelState.AddModelError(string.Empty, "Benutzername oder Passwort ist falsch!");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, _auth.Username),
            new(ClaimTypes.NameIdentifier, _auth.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost("/Account/Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }
}
