using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class PermissionsController(IUserSessionAccessor userSessionAccessor) : Controller
{
    [RequirePermission("formStock", PermissionMode.Read)]
    public IActionResult Index()
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        return View(currentUser);
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    public IActionResult EditDemo()
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        return View(currentUser);
    }
}
