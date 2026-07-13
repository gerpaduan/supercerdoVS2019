using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
[RequirePermission("formMovimientos", PermissionMode.Read)]
public class MovementsController(
    IUserSessionAccessor userSessionAccessor,
    IBranchLookupService branchLookupService,
    IMovementQueryService movementQueryService,
    IMovementCommandService movementCommandService,
    IMovementProductLookupService movementProductLookupService,
    IPermissionService permissionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        int originBranchId = 0,
        int destinationBranchId = 0,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var from = dateFrom?.Date ?? DateTime.Today.AddDays(-7);
        var to = dateTo?.Date ?? DateTime.Today;
        if (to < from)
        {
            to = from;
        }

        var branches = await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken);
        var items = await movementQueryService.GetMovementsAsync(
            currentUser.Company.CompanyId,
            new MovementListQuery
            {
                OriginBranchId = originBranchId,
                DestinationBranchId = destinationBranchId,
                DateFrom = from,
                DateTo = to
            },
            cancellationToken);

        return View(new MovementListPageViewModel
        {
            CanManageMovements = permissionService.CanEdit(currentUser, "formNuevoMovimiento", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId),
            OriginBranchId = originBranchId,
            DestinationBranchId = destinationBranchId,
            DateFrom = from,
            DateTo = to,
            Branches = branches,
            Items = items
        });
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var item = await movementQueryService.GetMovementByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        ViewBag.CanEdit = permissionService.CanEdit(
            currentUser,
            "formNuevoMovimiento",
            DateOnly.FromDateTime(item.MovementDate),
            currentUser.UserId);

        return View(item);
    }

    [RequirePermission("formNuevoMovimiento", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var branches = (await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken)).ToList();
        var quickProducts = (await movementProductLookupService.GetQuickProductsAsync(currentUser.Company.CompanyId, 1000, 1000, cancellationToken)).ToList();

        MovementEditViewModel model;
        if (id > 0)
        {
            var item = await movementQueryService.GetMovementByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return NotFound();
            }

            var canEdit = permissionService.CanEdit(
                currentUser,
                "formNuevoMovimiento",
                DateOnly.FromDateTime(item.MovementDate),
                currentUser.UserId);

            model = new MovementEditViewModel
            {
                MovementId = item.MovementId,
                IsEdit = true,
                IsInitiallyReadOnly = true,
                CanEnableEditing = canEdit,
                OriginBranchId = item.OriginBranchId,
                DestinationBranchId = item.DestinationBranchId,
                MovementDate = item.MovementDate,
                Notes = item.Notes,
                UserDisplayName = currentUser.DisplayName,
                CreatedAtText = item.CreatedAt.HasValue ? item.CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "-",
                CreatedByName = string.IsNullOrWhiteSpace(item.CreatedByName) ? "-" : item.CreatedByName,
                UpdatedAtText = item.UpdatedAt.HasValue ? item.UpdatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "-",
                UpdatedByName = string.IsNullOrWhiteSpace(item.UpdatedByName) ? "-" : item.UpdatedByName,
                OriginMovementReference = item.OriginMovementId.HasValue && item.OriginMovementId.Value > 0
                    ? item.OriginMovementId.Value.ToString(CultureInfo.InvariantCulture)
                    : item.MovementId.ToString(CultureInfo.InvariantCulture),
                DestinationMovementReference = item.OriginMovementId.HasValue && item.OriginMovementId.Value > 0
                    ? item.MovementId.ToString(CultureInfo.InvariantCulture)
                    : "-",
                Lines = item.Lines.Select(MapEditLine).ToList()
            };
        }
        else
        {
            model = new MovementEditViewModel
            {
                UserDisplayName = currentUser.DisplayName,
                OriginBranchId = currentUser.ActiveBranch.BranchId
            };

            if (branches.Count == 2 && currentUser.ActiveBranch.BranchId > 0)
            {
                var destinationBranch = branches.FirstOrDefault(x => x.BranchId != currentUser.ActiveBranch.BranchId);
                if (destinationBranch is not null)
                {
                    model.DestinationBranchId = destinationBranch.BranchId;
                }
            }
        }

        model.Branches = branches;
        model.QuickProducts = quickProducts;
        return View(model);
    }

    [RequirePermission("formNuevoMovimiento", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MovementEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model ??= new MovementEditViewModel();
        NormalizeEditModel(model);
        NormalizeLineDecimals(model);
        model.IsEdit = model.MovementId > 0;
        model.IsInitiallyReadOnly = false;

        if (!permissionService.CanEdit(
                currentUser,
                "formNuevoMovimiento",
                DateOnly.FromDateTime(model.MovementDate),
                currentUser.UserId))
        {
            return Forbid();
        }

        if (model.OriginBranchId == model.DestinationBranchId && model.OriginBranchId > 0)
        {
            ModelState.AddModelError(string.Empty, "La sucursal origen y destino deben ser diferentes.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto al movimiento.");
        }

        for (var index = 0; index < model.Lines.Count; index++)
        {
            var line = model.Lines[index];
            if (line.ProductId <= 0)
            {
                ModelState.AddModelError(string.Empty, $"La linea {index + 1} no tiene un producto valido.");
            }

            if (line.QuantityUnits < 0)
            {
                ModelState.AddModelError(string.Empty, $"La linea {index + 1} tiene unidades negativas.");
            }

            if (line.QuantityWeightKg <= 0)
            {
                ModelState.AddModelError(string.Empty, $"La linea {index + 1} debe tener kilos mayores a cero.");
            }
        }

        if (!ModelState.IsValid)
        {
            await ReloadEditListsAsync(model, currentUser.Company.CompanyId, cancellationToken);
            return View(model);
        }

        var result = await movementCommandService.SaveMovementAsync(
            new MovementSaveRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                UserId = currentUser.UserId,
                MovementId = model.MovementId,
                OriginBranchId = model.OriginBranchId,
                DestinationBranchId = model.DestinationBranchId,
                MovementDate = model.MovementDate,
                Notes = model.Notes,
                Lines = model.Lines.Select(line => new MovementSaveLineRequest
                {
                    ProductId = line.ProductId,
                    QuantityUnits = line.QuantityUnits,
                    QuantityWeightKg = line.QuantityWeightKg,
                    ScaleWeight = line.ScaleWeight,
                    AllowEntry = line.AllowEntry
                }).ToArray()
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            await ReloadEditListsAsync(model, currentUser.Company.CompanyId, cancellationToken);
            return View(model);
        }

        TempData["FlashSuccess"] = model.IsEdit
            ? "El movimiento se guardo correctamente."
            : "El movimiento se registro correctamente.";

        return RedirectToAction(nameof(Detail), new { id = result.MovementId });
    }

    [RequirePermission("formNuevoMovimiento", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> ProductByCode(long code, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida." });
        }

        var product = await movementProductLookupService.FindProductByCodeAsync(
            currentUser.Company.CompanyId,
            code,
            cancellationToken);

        if (product is null)
        {
            return Json(new { ok = false, message = "No existe un producto con ese codigo." });
        }

        return Json(new
        {
            ok = true,
            productId = product.ProductId,
            code = product.Code,
            description = product.Description,
            type = product.Type,
            weighable = product.Weighable,
            averageWeight = product.AverageWeight
        });
    }

    private static MovementEditLineViewModel MapEditLine(MovementDetailLineItem item)
    {
        return new MovementEditLineViewModel
        {
            ProductId = item.ProductId,
            Code = item.ProductCode,
            ProductName = item.ProductName,
            QuantityUnits = item.QuantityUnits,
            QuantityWeightKg = item.QuantityWeightKg,
            ScaleWeight = item.ScaleWeight,
            AllowEntry = item.AllowEntry
        };
    }

    private async Task ReloadEditListsAsync(MovementEditViewModel model, int companyId, CancellationToken cancellationToken)
    {
        model.Branches = (await branchLookupService.GetBranchesAsync(companyId, cancellationToken)).ToList();
        model.QuickProducts = (await movementProductLookupService.GetQuickProductsAsync(companyId, 1000, 1000, cancellationToken)).ToList();
    }

    private static void NormalizeEditModel(MovementEditViewModel model)
    {
        model.Notes = (model.Notes ?? string.Empty).Trim();

        foreach (var line in model.Lines)
        {
            line.ProductName = (line.ProductName ?? string.Empty).Trim();
            line.ProductType = (line.ProductType ?? string.Empty).Trim();
        }
    }

    private void NormalizeLineDecimals(MovementEditViewModel model)
    {
        if (model.Lines.Count == 0)
        {
            return;
        }

        for (var index = 0; index < model.Lines.Count; index++)
        {
            var line = model.Lines[index];
            var weightKey = $"Lines[{index}].QuantityWeightKg";
            var averageKey = $"Lines[{index}].AverageWeight";

            if (TryParseDecimalFlexible(Request.Form[weightKey], out var weight))
            {
                line.QuantityWeightKg = weight;
                ModelState.Remove(weightKey);
            }

            if (TryParseDecimalFlexible(Request.Form[averageKey], out var averageWeight))
            {
                line.AverageWeight = averageWeight;
                ModelState.Remove(averageKey);
            }
        }
    }

    private static bool TryParseDecimalFlexible(string? value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(normalized.Replace(".", ","), NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out result)
            || decimal.TryParse(normalized.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }
}
