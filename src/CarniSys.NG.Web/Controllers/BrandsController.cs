using CarniSys.NG.Application.People;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class BrandsController(
    IUserSessionAccessor userSessionAccessor,
    IBrandQueryService brandQueryService,
    IBrandCommandService brandCommandService) : Controller
{
    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await brandQueryService.GetBrandsAsync(
            new BrandListQuery
            {
                SearchText = searchText ?? string.Empty
            },
            cancellationToken);

        var scopedItems = items
            .Where(x => x.CompanyId == 0 || x.CompanyId == currentUser.Company.CompanyId)
            .ToArray();

        return View(new BrandListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            Items = scopedItems
        });
    }

    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var item = await brandQueryService.GetBrandByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (item.CompanyId != 0 && item.CompanyId != currentUser.Company.CompanyId)
        {
            return Forbid();
        }

        return View(item);
    }

    [RequirePermission("formNuevoCorte", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var isAdministrator = currentUser.IsAdministrator;
        BrandEditViewModel model;

        if (id > 0)
        {
            var item = await brandQueryService.GetBrandByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return NotFound();
            }

            if (item.CompanyId != 0 && item.CompanyId != currentUser.Company.CompanyId)
            {
                return Forbid();
            }

            model = new BrandEditViewModel
            {
                BrandId = item.BrandId,
                IsEdit = true,
                BrandName = item.BrandName,
                Notes = item.Notes,
                OwnerId = item.OwnerId,
                OwnerName = item.OwnerName,
                IsAdministrator = isAdministrator,
                IsNameReadOnly = !isAdministrator
            };
        }
        else
        {
            model = new BrandEditViewModel
            {
                IsAdministrator = isAdministrator
            };
        }

        return View(model);
    }

    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Lookup(string searchText = "", int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { items = Array.Empty<object>() });
        }

        var items = await brandQueryService.GetBrandsAsync(
            new BrandListQuery
            {
                SearchText = searchText ?? string.Empty,
                Skip = skip,
                Take = take
            },
            cancellationToken);

        var scopedItems = items
            .Where(x => x.CompanyId == 0 || x.CompanyId == currentUser.Company.CompanyId)
            .ToArray();

        return Json(new
        {
            hasMore = scopedItems.Length >= Math.Min(take <= 0 ? 50 : take, 100),
            items = scopedItems.Select(x => new
            {
                personId = x.BrandId,
                businessName = x.BrandName
            })
        });
    }

    [RequirePermission("formNuevoCorte", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BrandEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model.BrandName = (model.BrandName ?? string.Empty).Trim();
        model.Notes = (model.Notes ?? string.Empty).Trim();
        model.OwnerName = (model.OwnerName ?? string.Empty).Trim();
        model.IsEdit = model.BrandId > 0;
        model.IsAdministrator = currentUser.IsAdministrator;
        model.IsNameReadOnly = model.IsEdit && !currentUser.IsAdministrator;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await brandCommandService.SaveBrandAsync(
            new BrandSaveRequest
            {
                BrandId = model.BrandId,
                BrandName = model.BrandName,
                Notes = model.Notes,
                OwnerId = model.OwnerId,
                IsAdministrator = currentUser.IsAdministrator,
                ConfirmSimilarBrands = model.ConfirmSimilarBrands
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            if (result.RequiresConfirmation)
            {
                model.ConfirmSimilarBrands = true;
            }

            return View(model);
        }

        TempData["FlashSuccess"] = model.IsEdit
            ? $"La marca \"{model.BrandName}\" se actualizo correctamente."
            : $"La marca \"{model.BrandName}\" se guardo correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("formNuevoCorte", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int brandId, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var brand = await brandQueryService.GetBrandByIdAsync(brandId, cancellationToken);
        if (brand is null)
        {
            return NotFound();
        }

        var result = await brandCommandService.DeleteBrandAsync(
            currentUser.Company.CompanyId,
            brandId,
            cancellationToken);

        if (!result.Success)
        {
            TempData["FlashError"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["FlashSuccess"] = $"La marca \"{brand.BrandName}\" se elimino correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
