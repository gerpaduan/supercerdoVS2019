using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class ProductDetailsController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService,
    IPermissionService permissionService) : Controller
{
    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var item = await productQueryService.GetProductByIdAsync(currentUser.Company.CompanyId, id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        var model = new ProductDetailPageViewModel
        {
            Item = item,
            CanEdit = permissionService.CanEdit(currentUser, "formNuevoCorte", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId)
        };

        return View(model);
    }
}
