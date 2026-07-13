using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Infrastructure;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

public class AccessController(CarniSys.NG.Application.Authentication.IAuthenticationService authenticationService) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string returnUrl = "")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        model ??= new LoginViewModel();
        model.UserName = (model.UserName ?? string.Empty).Trim();
        model.ReturnUrl ??= string.Empty;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authenticationService.LoginAsync(
            new LoginRequest(model.UserName, model.Password),
            cancellationToken);

        if (!result.IsAuthenticated || result.User is null)
        {
            model.Password = string.Empty;
            model.Error = result.FailureMessage ?? "No fue posible iniciar sesion.";
            return View(model);
        }

        var principal = AuthenticatedUserClaimsPrincipalFactory.Create(result.User, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Denied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
