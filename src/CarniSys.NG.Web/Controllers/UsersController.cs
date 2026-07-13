using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Application.Users;
using CarniSys.NG.Infrastructure;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class UsersController(
    IUserSessionAccessor userSessionAccessor,
    IUserQueryService userQueryService,
    IUserCommandService userCommandService,
    IBranchLookupService branchLookupService,
    IPermissionService permissionService,
    CarniSys.NG.Application.Authentication.IAuthenticationService authenticationService) : Controller
{
    [RequirePermission("FormUsuarios", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", bool onlyActive = false, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await userQueryService.GetUsersAsync(
            currentUser.Company.CompanyId,
            new UserListQuery
            {
                SearchText = searchText ?? string.Empty,
                OnlyActive = onlyActive
            },
            cancellationToken);

        var model = new UserListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            OnlyActive = onlyActive,
            CanManageUsers = permissionService.CanEdit(currentUser, "FormNuevoUsuario", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId),
            Items = items
        };

        return View(model);
    }

    [RequirePermission("FormUsuarios", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var item = await userQueryService.GetUserByIdAsync(currentUser.Company.CompanyId, id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [RequirePermission("FormNuevoUsuario", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        UserEditViewModel model;
        if (id > 0)
        {
            var item = await userQueryService.GetUserByIdAsync(currentUser.Company.CompanyId, id, cancellationToken);
            if (item is null)
            {
                return NotFound();
            }

            model = new UserEditViewModel
            {
                UserId = item.UserId,
                IsEdit = true,
                FullName = item.FullName,
                Login = item.Login,
                IsAdministrator = item.IsAdministrator,
                IsActive = item.IsActive,
                Email = item.Email,
                BranchId = item.BranchId,
                CanLoginOutsideBranch = item.CanLoginOutsideBranch,
                CompanyId = item.CompanyId
            };
        }
        else
        {
            model = new UserEditViewModel
            {
                CompanyId = currentUser.Company.CompanyId,
                BranchId = currentUser.ActiveBranch.BranchId,
                IsActive = true
            };
        }

        await LoadBranchesAsync(model, cancellationToken);
        return View(model);
    }

    [RequirePermission("FormNuevoUsuario", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model.CompanyId = currentUser.Company.CompanyId;
        model.FullName = (model.FullName ?? string.Empty).Trim();
        model.Login = (model.Login ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim();
        model.Password ??= string.Empty;

        if (model.UserId == 0 && string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "La clave es obligatoria para un usuario nuevo.");
        }

        if (model.Password.Contains(' '))
        {
            ModelState.AddModelError(nameof(model.Password), "La clave no puede contener espacios en blanco.");
        }

        if (!ModelState.IsValid)
        {
            await LoadBranchesAsync(model, cancellationToken);
            return View(model);
        }

        var result = await userCommandService.SaveUserAsync(
            new SaveUserRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                UserId = model.UserId,
                FullName = model.FullName,
                Login = model.Login,
                Password = model.Password,
                IsAdministrator = model.IsAdministrator,
                IsActive = model.IsActive,
                Email = model.Email,
                BranchId = model.BranchId,
                CanLoginOutsideBranch = model.CanLoginOutsideBranch
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            await LoadBranchesAsync(model, cancellationToken);
            return View(model);
        }

        if (currentUser.UserId == result.UserId)
        {
            var refreshedUser = await authenticationService.GetUserByIdAsync(result.UserId, cancellationToken);
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

        TempData["FlashSuccess"] = model.UserId > 0
            ? $"El usuario \"{model.FullName}\" se actualizo correctamente."
            : $"El usuario \"{model.FullName}\" se creo correctamente.";

        return RedirectToAction(nameof(Detail), new { id = result.UserId });
    }

    private async Task LoadBranchesAsync(UserEditViewModel model, CancellationToken cancellationToken)
    {
        var items = await branchLookupService.GetBranchesAsync(model.CompanyId, cancellationToken);
        model.Branches = items
            .Select(x => new SelectListItem
            {
                Value = x.BranchId.ToString(),
                Text = x.BranchName,
                Selected = x.BranchId == model.BranchId
            })
            .ToList();
    }
}
