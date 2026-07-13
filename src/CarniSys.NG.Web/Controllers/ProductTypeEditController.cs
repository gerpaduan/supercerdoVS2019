using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class ProductTypeEditController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService,
    IProductTypeCommandService productTypeCommandService) : Controller
{
    [RequirePermission("formAddOrEditTipoProducto", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Index(string type = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        ProductTypeEditViewModel model;
        var normalizedType = (type ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedType))
        {
            model = new ProductTypeEditViewModel
            {
                SortOrder = 100
            };
        }
        else
        {
            var items = await productQueryService.GetCompanyProductTypesAsync(
                currentUser.Company.CompanyId,
                new ProductTypeListQuery
                {
                    SearchText = normalizedType
                },
                cancellationToken);

            var item = items.FirstOrDefault(x => string.Equals(x.TypeName, normalizedType, StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return NotFound();
            }

            if (item.IsReserved)
            {
                return Forbid();
            }

            model = new ProductTypeEditViewModel
            {
                OriginalTypeName = item.TypeName,
                IsEdit = true,
                TypeName = item.TypeName,
                SortOrder = item.SortOrder,
                IsReserved = item.IsReserved
            };
        }

        return View(model);
    }

    [RequirePermission("formAddOrEditTipoProducto", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProductTypeEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model.OriginalTypeName = (model.OriginalTypeName ?? string.Empty).Trim();
        model.TypeName = (model.TypeName ?? string.Empty).Trim();
        model.IsEdit = !string.IsNullOrWhiteSpace(model.OriginalTypeName);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.IsEdit)
        {
            var currentItems = await productQueryService.GetCompanyProductTypesAsync(
                currentUser.Company.CompanyId,
                new ProductTypeListQuery
                {
                    SearchText = model.OriginalTypeName
                },
                cancellationToken);

            var currentItem = currentItems.FirstOrDefault(x => string.Equals(x.TypeName, model.OriginalTypeName, StringComparison.OrdinalIgnoreCase));
            if (currentItem is null)
            {
                return NotFound();
            }

            if (currentItem.IsReserved)
            {
                return Forbid();
            }
        }

        var result = await productTypeCommandService.SaveCompanyProductTypeAsync(
            new ProductTypeEditRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                OriginalTypeName = model.OriginalTypeName,
                TypeName = model.TypeName,
                SortOrder = model.SortOrder
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        TempData["FlashSuccess"] = model.IsEdit
            ? $"El tipo de producto \"{model.TypeName}\" se actualizo correctamente."
            : $"El tipo de producto \"{model.TypeName}\" se registro correctamente.";

        return RedirectToAction("Index", "ProductTypes");
    }

    [RequirePermission("formAddOrEditTipoProducto", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string typeName, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var normalizedTypeName = (typeName ?? string.Empty).Trim();
        var result = await productTypeCommandService.DeleteCompanyProductTypeAsync(
            currentUser.Company.CompanyId,
            normalizedTypeName,
            cancellationToken);

        if (!result.Success)
        {
            TempData["FlashError"] = result.ErrorMessage;
            return RedirectToAction("Index", "ProductTypes");
        }

        TempData["FlashSuccess"] = $"El tipo de producto \"{normalizedTypeName}\" se elimino correctamente.";
        return RedirectToAction("Index", "ProductTypes");
    }
}
