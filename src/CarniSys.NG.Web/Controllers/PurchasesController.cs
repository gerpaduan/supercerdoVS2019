using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Purchases;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
[RequirePermission("formCompras", PermissionMode.Read)]
public class PurchasesController(
    IUserSessionAccessor userSessionAccessor,
    IBranchLookupService branchLookupService,
    IMovementProductLookupService movementProductLookupService,
    IPurchaseQueryService purchaseQueryService,
    IPurchaseCommandService purchaseCommandService,
    IPermissionService permissionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        int branchId = 0,
        string purchaseType = "Todos",
        string searchText = "",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var from = dateFrom?.Date ?? DateTime.Today;
        var to = dateTo?.Date ?? DateTime.Today;
        if (to < from)
        {
            to = from;
        }

        var normalizedType = NormalizePurchaseType(purchaseType);
        var selectedBranchId = branchId > 0
            ? branchId
            : currentUser.ActiveBranch.BranchId > 0
                ? currentUser.ActiveBranch.BranchId
                : 0;
        var branches = await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken);
        var items = await purchaseQueryService.GetPurchasesAsync(
            new PurchaseListQuery
            {
                BranchId = selectedBranchId,
                PurchaseType = normalizedType,
                SearchText = searchText ?? string.Empty,
                DateFrom = from,
                DateTo = to
            },
            cancellationToken);

        return View(new PurchaseListPageViewModel
        {
            BranchId = selectedBranchId,
            PurchaseType = normalizedType,
            SearchText = searchText ?? string.Empty,
            DateFrom = from,
            DateTo = to,
            TotalHalfCarcassCount = items.Sum(x => x.HalfCarcassCount),
            TotalKg = items.Sum(x => x.TotalKg),
            TotalAmount = items.Sum(x => x.TotalAmount),
            CanCreatePurchase = permissionService.CanEdit(currentUser, "formNuevaCompra", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId),
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

        var item = await purchaseQueryService.GetPurchaseByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        ViewBag.CanEditPurchase =
            IsSupportedPurchaseType(item.PurchaseType)
            && permissionService.CanEdit(
                currentUser,
                "formModificarCompra",
                DateOnly.FromDateTime(item.PurchaseDate),
                item.CreatedByUserId == 0 ? currentUser.UserId : item.CreatedByUserId);

        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var branches = await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken);

        if (id <= 0)
        {
            if (!permissionService.CanEdit(currentUser, "formNuevaCompra", DateOnly.FromDateTime(DateTime.Today), currentUser.UserId))
            {
                TempData["FlashError"] = "No tiene permisos para registrar compras.";
                return RedirectToAction(nameof(Index));
            }

            return View(BuildCreateModel(branches, currentUser.ActiveBranch.BranchId));
        }

        var item = await purchaseQueryService.GetPurchaseByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        if (!IsSupportedPurchaseType(item.PurchaseType))
        {
            TempData["FlashError"] = "Por ahora NG solo permite editar compras tipo Cortes o Media Res.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        if (IsCancelledStatus(item.Status))
        {
            TempData["FlashError"] = "No se puede editar una compra anulada.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        if (!permissionService.CanEdit(
            currentUser,
            "formModificarCompra",
            DateOnly.FromDateTime(item.PurchaseDate),
            item.CreatedByUserId == 0 ? currentUser.UserId : item.CreatedByUserId))
        {
            TempData["FlashError"] = "No tiene permisos para modificar esta compra.";
            return RedirectToAction(nameof(Detail), new { id });
        }

        return View(BuildEditModel(item, branches));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PurchaseEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model ??= new PurchaseEditViewModel();
        NormalizeModel(model);

        PurchaseDetailItem? existing = null;
        if (model.PurchaseId > 0)
        {
            existing = await purchaseQueryService.GetPurchaseByIdAsync(model.PurchaseId, cancellationToken);
            if (existing is null)
            {
                TempData["FlashError"] = "No se encontro la compra a modificar.";
                return RedirectToAction(nameof(Index));
            }

            if (!IsSupportedPurchaseType(existing.PurchaseType))
            {
                TempData["FlashError"] = "Por ahora NG solo permite editar compras tipo Cortes o Media Res.";
                return RedirectToAction(nameof(Detail), new { id = model.PurchaseId });
            }

            if (IsCancelledStatus(existing.Status))
            {
                TempData["FlashError"] = "No se puede editar una compra anulada.";
                return RedirectToAction(nameof(Detail), new { id = model.PurchaseId });
            }
        }

        var permissionResource = model.PurchaseId > 0 ? "formModificarCompra" : "formNuevaCompra";
        var permissionDate = DateOnly.FromDateTime(model.PurchaseDate == DateTime.MinValue ? DateTime.Today : model.PurchaseDate);
        var ownerUserId = existing?.CreatedByUserId == 0 || existing is null ? currentUser.UserId : existing.CreatedByUserId;
        if (!permissionService.CanEdit(currentUser, permissionResource, permissionDate, ownerUserId))
        {
            TempData["FlashError"] = "No tiene permisos para guardar esta compra.";
            return RedirectToAction(model.PurchaseId > 0 ? nameof(Detail) : nameof(Index), new { id = model.PurchaseId });
        }

        ValidateModel(model);

        var branches = await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken);
        model.Branches = branches;
        model.IsEdit = model.PurchaseId > 0;
        model.AvailablePurchaseTypes = GetAvailablePurchaseTypes();
        model.CanUseHalfCarcass = true;
        if (existing is not null)
        {
            ApplyAuditLabels(model, existing);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await purchaseCommandService.SavePurchaseAsync(
            new PurchaseSaveRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                PurchaseId = model.PurchaseId,
                PurchaseType = model.PurchaseType,
                BranchId = model.BranchId,
                PurchaseDate = model.PurchaseDate,
                SupplierId = model.SupplierId,
                ReceiptNumber = model.ReceiptNumber,
                Notes = model.Notes,
                CurrentAccount = model.CurrentAccount,
                HalfCarcassCount = model.HalfCarcassCount,
                UserId = currentUser.UserId,
                Lines = model.Lines
                    .Select(x => new PurchaseSaveLineRequest
                    {
                        LineType = x.LineType,
                        ProductId = x.ProductId,
                        TroopNumber = x.TroopNumber,
                        QuantityKg = x.QuantityKg,
                        PricePerKg = x.PricePerKg
                    })
                    .ToList()
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            return View(model);
        }

        TempData["FlashSuccess"] = model.PurchaseId > 0
            ? "La compra se guardo correctamente."
            : "La compra se registro correctamente.";

        return RedirectToAction(nameof(Detail), new { id = result.PurchaseId });
    }

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

    private static string NormalizePurchaseType(string? purchaseType)
    {
        var normalized = (purchaseType ?? string.Empty).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? "Todos" : normalized;
    }

    private static bool IsCancelledStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim();
        return normalized.Equals("Anulado", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Anulada", StringComparison.OrdinalIgnoreCase);
    }

    private static PurchaseEditViewModel BuildCreateModel(IReadOnlyCollection<BranchLookupItem> branches, int activeBranchId)
    {
        var selectedBranchId = activeBranchId > 0
            ? activeBranchId
            : branches.FirstOrDefault()?.BranchId ?? 0;

        return new PurchaseEditViewModel
        {
            BranchId = selectedBranchId,
            PurchaseDate = DateTime.Now,
            PurchaseType = "Cortes",
            CanUseHalfCarcass = true,
            AvailablePurchaseTypes = GetAvailablePurchaseTypes(),
            Branches = branches
        };
    }

    private static PurchaseEditViewModel BuildEditModel(PurchaseDetailItem item, IReadOnlyCollection<BranchLookupItem> branches)
    {
        var model = new PurchaseEditViewModel
        {
            PurchaseId = item.PurchaseId,
            IsEdit = true,
            PurchaseType = item.PurchaseType,
            CanUseHalfCarcass = true,
            AvailablePurchaseTypes = GetAvailablePurchaseTypes(item.PurchaseType),
            BranchId = item.BranchId,
            PurchaseDate = item.PurchaseDate,
            SupplierId = item.SupplierId,
            SupplierName = item.SupplierName,
            SupplierTaxId = item.SupplierTaxId,
            CurrentAccount = item.CurrentAccount,
            HalfCarcassCount = item.HalfCarcassCount,
            ReceiptNumber = item.ReceiptNumber,
            Notes = item.Notes,
            Branches = branches,
            Lines = item.Lines
                .Select(x => new PurchaseEditLineViewModel
                {
                    LineType = x.LineType,
                    ProductId = x.ProductId ?? 0,
                    Code = x.Code,
                    ProductName = string.IsNullOrWhiteSpace(x.ProductName) ? (x.LineType == "MediaRes" ? "Media Res" : string.Empty) : x.ProductName,
                    TroopNumber = x.LineType == "MediaRes" ? x.Code : string.Empty,
                    QuantityKg = x.QuantityKg,
                    PricePerKg = x.Price
                })
                .ToList()
        };

        ApplyAuditLabels(model, item);
        return model;
    }

    private static void ApplyAuditLabels(PurchaseEditViewModel model, PurchaseDetailItem item)
    {
        model.CreatedAtLabel = item.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
        model.CreatedByLabel = item.CreatedByName;
        model.UpdatedAtLabel = item.UpdatedAt?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
        model.UpdatedByLabel = item.UpdatedByName;
    }

    private void ValidateModel(PurchaseEditViewModel model)
    {
        var isHalfCarcass = IsHalfCarcassType(model.PurchaseType);

        if (model.PurchaseDate == DateTime.MinValue)
        {
            ModelState.AddModelError(nameof(model.PurchaseDate), "Debe ingresar una fecha valida.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(nameof(model.Lines), "Debe ingresar al menos una linea.");
            return;
        }

        if (isHalfCarcass && (!model.HalfCarcassCount.HasValue || model.HalfCarcassCount.Value <= 0))
        {
            ModelState.AddModelError(nameof(model.HalfCarcassCount), "Debe ingresar la cantidad de medias.");
        }

        for (var i = 0; i < model.Lines.Count; i++)
        {
            var line = model.Lines[i];
            var lineNumber = i + 1;

            if (!isHalfCarcass && line.ProductId <= 0)
            {
                ModelState.AddModelError(nameof(model.Lines), $"La linea {lineNumber} no tiene un producto valido.");
            }

            if (line.QuantityKg <= 0)
            {
                ModelState.AddModelError(nameof(model.Lines), $"La linea {lineNumber} debe tener una cantidad mayor a cero.");
            }

            if (line.PricePerKg <= 0)
            {
                ModelState.AddModelError(nameof(model.Lines), $"La linea {lineNumber} debe tener un precio de compra mayor a cero.");
            }
        }

        if (!isHalfCarcass)
        {
            var duplicateProducts = model.Lines
                .Where(x => x.ProductId > 0)
                .GroupBy(x => x.ProductId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateProducts.Count > 0)
            {
                ModelState.AddModelError(nameof(model.Lines), "No se puede repetir el mismo producto en mas de una linea.");
            }
        }
    }

    private static void NormalizeModel(PurchaseEditViewModel model)
    {
        model.SupplierName = (model.SupplierName ?? string.Empty).Trim();
        model.SupplierTaxId = (model.SupplierTaxId ?? string.Empty).Trim();
        model.ReceiptNumber = (model.ReceiptNumber ?? string.Empty).Trim();
        model.Notes = (model.Notes ?? string.Empty).Trim();
        model.PurchaseType = NormalizeEditPurchaseType(model.PurchaseType);
        model.Lines = model.Lines
            .Where(x =>
                x.ProductId > 0
                || !string.IsNullOrWhiteSpace(x.TroopNumber)
                || !string.IsNullOrWhiteSpace(x.ProductName)
                || x.QuantityKg > 0
                || x.PricePerKg > 0)
            .Select(x => new PurchaseEditLineViewModel
            {
                LineType = NormalizeLineType(x.LineType, model.PurchaseType),
                ProductId = x.ProductId,
                Code = (x.Code ?? string.Empty).Trim(),
                ProductName = (x.ProductName ?? string.Empty).Trim(),
                TroopNumber = (x.TroopNumber ?? string.Empty).Trim(),
                QuantityKg = x.QuantityKg,
                PricePerKg = x.PricePerKg
            })
            .ToList();

        if (IsHalfCarcassType(model.PurchaseType))
        {
            model.HalfCarcassCount = model.HalfCarcassCount.GetValueOrDefault() > 0
                ? model.HalfCarcassCount
                : model.Lines.Count;

            foreach (var line in model.Lines)
            {
                line.ProductId = 0;
                line.Code = line.TroopNumber;
                line.ProductName = "Media Res";
            }
        }
        else
        {
            model.HalfCarcassCount = null;
            foreach (var line in model.Lines)
            {
                line.TroopNumber = string.Empty;
            }
        }
    }

    private static IReadOnlyCollection<string> GetAvailablePurchaseTypes(string? currentType = null)
    {
        var items = new List<string> { "Cortes", "Media Res" };
        var normalized = NormalizeEditPurchaseType(currentType);
        if (!string.IsNullOrWhiteSpace(normalized) && !items.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(normalized);
        }

        return items;
    }

    private static bool IsSupportedPurchaseType(string? purchaseType)
    {
        return string.Equals(purchaseType, "Cortes", StringComparison.OrdinalIgnoreCase)
            || string.Equals(purchaseType, "Media Res", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsHalfCarcassType(string? purchaseType)
    {
        return string.Equals(purchaseType, "Media Res", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeEditPurchaseType(string? purchaseType)
    {
        return IsHalfCarcassType(purchaseType) ? "Media Res" : "Cortes";
    }

    private static string NormalizeLineType(string? lineType, string? purchaseType)
    {
        return IsHalfCarcassType(purchaseType) || string.Equals(lineType, "MediaRes", StringComparison.OrdinalIgnoreCase)
            ? "MediaRes"
            : "Corte";
    }
}
