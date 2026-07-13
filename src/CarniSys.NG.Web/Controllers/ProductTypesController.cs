using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class ProductTypesController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService,
    IPermissionService permissionService) : Controller
{
    [RequirePermission("formTiposProducto", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await productQueryService.GetCompanyProductTypesAsync(
            currentUser.Company.CompanyId,
            new ProductTypeListQuery
            {
                SearchText = searchText ?? string.Empty
            },
            cancellationToken);

        return View(new ProductTypeListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            IsGlobalCatalog = false,
            CanManageTypes = permissionService.CanEdit(currentUser, "formAddOrEditTipoProducto", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId),
            Items = items
        });
    }
}
