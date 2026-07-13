using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class GlobalProductTypesController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService,
    IProductTypeCommandService productTypeCommandService) : Controller
{
    [RequirePermission("formAddOrEditTipoProducto", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await productQueryService.GetGlobalProductTypesAsync(
            new ProductTypeListQuery
            {
                SearchText = searchText ?? string.Empty
            },
            cancellationToken);

        var companyTypes = await productQueryService.GetCompanyProductTypesAsync(
            currentUser.Company.CompanyId,
            new ProductTypeListQuery(),
            cancellationToken);

        return View("~/Views/ProductTypes/Index.cshtml", new ProductTypeListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            IsGlobalCatalog = true,
            CanManageTypes = true,
            ExistingCompanyTypes = companyTypes.Select(x => x.TypeName).ToHashSet(StringComparer.OrdinalIgnoreCase),
            Items = items
        });
    }

    [RequirePermission("formAddOrEditTipoProducto", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ImportGlobalProductTypesViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var result = await productTypeCommandService.ImportGlobalProductTypesAsync(
            currentUser.Company.CompanyId,
            model?.Types?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray() ?? Array.Empty<string>(),
            cancellationToken);

        if (!result.Success)
        {
            TempData["FlashError"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["FlashSuccess"] = "Los tipos globales seleccionados se importaron correctamente.";
        return RedirectToAction("Index", "ProductTypes");
    }
}
