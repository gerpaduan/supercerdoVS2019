using CarniSys.NG.Application.People;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class ProductEditController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService,
    IBrandQueryService brandQueryService) : Controller
{
    [RequirePermission("formNuevoCorte", PermissionMode.Edit)]
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

        var branchStockPoints = await productQueryService.GetBranchStockPointsAsync(
            currentUser.Company.CompanyId,
            item.ProductId,
            item.StockPoint,
            cancellationToken);

        var model = new ProductEditViewModel
        {
            ProductId = item.ProductId,
            Code = item.Code,
            Description = item.Description,
            BrandId = item.BrandId,
            BrandName = item.BrandName,
            PricePerKilogram = item.PricePerKilogram,
            Weighable = item.Weighable,
            AverageWeight = item.AverageWeight,
            StockPoint = item.StockPoint,
            UseBranchStockPoints = branchStockPoints.HasCustomBranchPoints,
            BranchStockPoints = branchStockPoints.Items.Select(x => new ProductBranchStockPointEditViewModel
            {
                BranchId = x.BranchId,
                BranchName = x.BranchName,
                StockPoint = x.StockPoint
            }).ToList(),
            IncludedInStockClosing = item.IncludedInStockClosing,
            Enabled = item.Enabled,
            QuickElaboratedEntry = item.QuickElaboratedEntry
        };

        return View(model);
    }

    [RequirePermission("formNuevoCorte", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProductEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model.BrandName = (model.BrandName ?? string.Empty).Trim();

        if (model.BrandId.HasValue && model.BrandId.Value > 0)
        {
            var brand = await brandQueryService.GetBrandByIdAsync(model.BrandId.Value, cancellationToken);
            if (brand is null || (brand.CompanyId != 0 && brand.CompanyId != currentUser.Company.CompanyId))
            {
                ModelState.AddModelError(nameof(model.BrandName), "La marca seleccionada no es valida para la empresa actual.");
            }
            else
            {
                model.BrandName = brand.BrandName;
            }
        }
        else
        {
            model.BrandId = null;
            model.BrandName = string.Empty;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await productQueryService.UpdateProductBasicsAsync(
            new ProductEditRequest
            {
                ProductId = model.ProductId,
                CompanyId = currentUser.Company.CompanyId,
                BrandId = model.BrandId,
                PricePerKilogram = model.PricePerKilogram,
                Weighable = model.Weighable,
                AverageWeight = model.AverageWeight,
                StockPoint = model.StockPoint,
                UseBranchStockPoints = model.UseBranchStockPoints,
                BranchStockPoints = (model.BranchStockPoints ?? []).Select(x => new ProductBranchStockPointItem
                {
                    BranchId = x.BranchId,
                    BranchName = x.BranchName,
                    StockPoint = x.StockPoint
                }).ToArray(),
                IncludedInStockClosing = model.IncludedInStockClosing,
                Enabled = model.Enabled,
                QuickElaboratedEntry = model.QuickElaboratedEntry
            },
            cancellationToken);

        if (!updated)
        {
            ModelState.AddModelError(string.Empty, "No fue posible guardar el producto en la empresa actual.");
            return View(model);
        }

        TempData["FlashSuccess"] = $"El producto \"{model.Description}\" se actualizo correctamente.";
        return RedirectToAction("Index", "ProductDetails", new { id = model.ProductId });
    }
}
