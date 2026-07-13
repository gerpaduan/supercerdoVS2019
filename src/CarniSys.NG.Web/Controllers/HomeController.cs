using System.Diagnostics;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class HomeController(IUserSessionAccessor userSessionAccessor, IPermissionService permissionService) : Controller
{
    public IActionResult Index()
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var probes = new[]
        {
            BuildProbe("Stock", "formStock", "formAddOrEditStock", currentUser, today),
            BuildProbe("Ventas", "formVentas", "formNuevaVenta", currentUser, today),
            BuildProbe("Finanzas", "formCtasCtes", "formAddOrEditPago", currentUser, today),
            BuildProbe("Productos", "formCortes", "formNuevoCorte", currentUser, today)
        };

        var model = new HomeIndexViewModel
        {
            UserName = currentUser.DisplayName,
            Login = currentUser.Login.Value,
            CompanyName = currentUser.Company.Name,
            BranchName = currentUser.ActiveBranch.Name,
            PermissionProbes = probes
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private PermissionProbeViewModel BuildProbe(
        string label,
        string readResource,
        string editResource,
        CarniSys.NG.Domain.Authentication.AuthenticatedUser currentUser,
        DateOnly operationDate)
    {
        return new PermissionProbeViewModel
        {
            Label = label,
            Resource = $"{readResource} / {editResource}",
            CanRead = permissionService.CanRead(currentUser, readResource, operationDate),
            CanEdit = permissionService.CanEdit(currentUser, editResource, operationDate, currentUser.UserId)
        };
    }
}
