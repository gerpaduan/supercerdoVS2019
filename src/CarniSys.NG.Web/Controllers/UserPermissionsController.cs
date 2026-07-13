using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Application.Users;
using CarniSys.NG.Infrastructure;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class UserPermissionsController(
    IUserSessionAccessor userSessionAccessor,
    IUserQueryService userQueryService,
    IUserPermissionCommandService userPermissionCommandService,
    CarniSys.NG.Application.Authentication.IAuthenticationService authenticationService) : Controller
{
    [RequirePermission("FormNuevoUsuario", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var model = await userQueryService.GetUserPermissionsAsync(
            currentUser.Company.CompanyId,
            id,
            cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(MapEditModel(model));
    }

    [RequirePermission("FormNuevoUsuario", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UserPermissionEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        NormalizeModel(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var saved = await userPermissionCommandService.SaveUserPermissionsAsync(
            new SaveUserPermissionsRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                UserId = model.UserId,
                Items = model.Items
                    .Select(item => new SaveUserPermissionItem
                    {
                        FormId = item.FormId,
                        CanRead = item.CanRead,
                        ReadDays = item.CanRead ? item.ReadDays : 0,
                        CanEdit = item.CanEdit,
                        EditDays = item.CanEdit ? item.EditDays : 0,
                        OwnRecordsOnly = item.CanEdit && item.OwnRecordsOnly
                    })
                    .ToArray()
            },
            cancellationToken);

        if (!saved)
        {
            ModelState.AddModelError(string.Empty, "No fue posible guardar los permisos para el usuario seleccionado.");
            return View(model);
        }

        if (currentUser.UserId == model.UserId)
        {
            var refreshedUser = await authenticationService.GetUserByIdAsync(model.UserId, cancellationToken);
            if (refreshedUser is not null)
            {
                var principal = AuthenticatedUserClaimsPrincipalFactory.Create(
                    refreshedUser,
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true
                    });
            }
        }

        TempData["FlashSuccess"] = $"Los permisos de \"{model.UserName}\" se guardaron correctamente.";
        return RedirectToAction(nameof(Index), new { id = model.UserId });
    }

    private static UserPermissionEditViewModel MapEditModel(UserPermissionPage page)
    {
        return new UserPermissionEditViewModel
        {
            UserId = page.UserId,
            UserName = page.UserName,
            UserLogin = page.UserLogin,
            Items = page.Items
                .Select(item => new UserPermissionEditItemViewModel
                {
                    FormId = item.FormId,
                    FormName = item.FormName,
                    Description = item.Description,
                    CanRead = item.CanRead,
                    ReadDays = item.ReadDays,
                    CanEdit = item.CanEdit,
                    EditDays = item.EditDays,
                    OwnRecordsOnly = item.OwnRecordsOnly
                })
                .ToList()
        };
    }

    private static void NormalizeModel(UserPermissionEditViewModel model)
    {
        model.UserName = (model.UserName ?? string.Empty).Trim();
        model.UserLogin = (model.UserLogin ?? string.Empty).Trim();

        foreach (var item in model.Items)
        {
            item.FormName = (item.FormName ?? string.Empty).Trim();
            item.Description = (item.Description ?? string.Empty).Trim();

            if (!item.CanRead)
            {
                item.ReadDays = 0;
            }

            if (!item.CanEdit)
            {
                item.EditDays = 0;
                item.OwnRecordsOnly = true;
            }
        }
    }
}
