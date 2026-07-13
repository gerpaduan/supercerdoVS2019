using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Application.Stock;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
[RequirePermission("formStock", PermissionMode.Read)]
public class StockController(
    IUserSessionAccessor userSessionAccessor,
    IBranchLookupService branchLookupService,
    IStockCommandService stockCommandService,
    IStockQueryService stockQueryService,
    IMovementProductLookupService movementProductLookupService,
    IPermissionService permissionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string searchText = "",
        int branchId = 0,
        DateTime? untilDate = null,
        string type = "",
        bool onlyWithStock = false,
        string stockState = "Todos",
        CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var branches = (await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken)).ToList();
        var selectedBranchId = branchId > 0
            ? branchId
            : currentUser.ActiveBranch.BranchId > 0
                ? currentUser.ActiveBranch.BranchId
                : 0;

        var effectiveUntilDate = untilDate ?? DateTime.Now;
        var result = await stockQueryService.GetStockMatrixAsync(
            currentUser.Company.CompanyId,
            new StockMatrixQuery
            {
                SearchText = searchText ?? string.Empty,
                BranchId = selectedBranchId,
                UntilDate = effectiveUntilDate,
                Type = type ?? string.Empty,
                OnlyWithStock = onlyWithStock,
                StockState = stockState ?? "Todos"
            },
            cancellationToken);

        return View(new StockListPageViewModel
        {
            CanManageStock = permissionService.CanEdit(
                currentUser,
                "formAddOrEditStock",
                DateOnly.FromDateTime(DateTime.Today),
                currentUser.UserId),
            SearchText = searchText ?? string.Empty,
            BranchId = selectedBranchId,
            UntilDate = effectiveUntilDate,
            Type = type ?? string.Empty,
            OnlyWithStock = onlyWithStock,
            StockState = string.IsNullOrWhiteSpace(stockState) ? "Todos" : stockState,
            Branches = branches,
            StateOptions = StockListPageViewModel.DefaultStateOptions,
            Result = result
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(string tipoCompra = "Ingreso Stock", string stockOperationType = "", int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var normalizedType = NormalizeOperationType(!string.IsNullOrWhiteSpace(stockOperationType) ? stockOperationType : tipoCompra);
        var branches = await branchLookupService.GetBranchesAsync(currentUser.Company.CompanyId, cancellationToken);
        StockEditDetailItem? stockDetail = null;

        if (id > 0)
        {
            stockDetail = await stockQueryService.GetStockEditByIdAsync(
                currentUser.Company.CompanyId,
                id,
                cancellationToken);

            if (stockDetail is null)
            {
                TempData["FlashError"] = "No se encontro el movimiento de stock.";
                return RedirectToAction(nameof(Index));
            }

            normalizedType = NormalizeOperationType(stockDetail.StockOperationType);
        }

        if (string.IsNullOrWhiteSpace(normalizedType))
        {
            return RedirectToAction(nameof(Index));
        }

        var selectedBranchId = stockDetail?.BranchId
            ?? (currentUser.ActiveBranch.BranchId > 0
                ? currentUser.ActiveBranch.BranchId
                : branches.FirstOrDefault()?.BranchId ?? 0);
        var selectedBranch = branches.FirstOrDefault(x => x.BranchId == selectedBranchId);
        var quickProducts = (await movementProductLookupService.GetQuickProductsAsync(
            currentUser.Company.CompanyId,
            1000,
            1000,
            cancellationToken)).ToList();

        var model = new StockEditViewModel
        {
            StockId = id,
            IsEdit = id > 0,
            StockOperationType = normalizedType,
            BranchId = selectedBranchId,
            BranchName = stockDetail?.BranchName ?? selectedBranch?.BranchName ?? string.Empty,
            OperationDate = stockDetail?.OperationDate ?? DateTime.Now,
            Notes = stockDetail?.Notes ?? string.Empty,
            Status = stockDetail?.Status ?? string.Empty,
            SupplierId = stockDetail?.SupplierId ?? 0,
            SupplierName = stockDetail?.SupplierName ?? string.Empty,
            SupplierTaxId = stockDetail?.SupplierTaxId ?? string.Empty,
            HalfCarcassCount = stockDetail?.HalfCarcassCount,
            HalfCarcassWeightKg = stockDetail?.HalfCarcassWeightKg,
            LinkedWeighingId = stockDetail?.LinkedWeighingId,
            LinkedPurchaseDate = stockDetail?.LinkedPurchaseDate,
            LinkedPurchaseSupplierName = stockDetail?.LinkedPurchaseSupplierName ?? string.Empty,
            LinkedPurchaseHalfCarcassCount = stockDetail?.LinkedPurchaseHalfCarcassCount,
            LinkedPurchaseWeightKg = stockDetail?.LinkedPurchaseWeightKg,
            AdjustedWeighingDate = stockDetail?.AdjustedWeighingDate,
            AdjustedWeighingSupplierName = stockDetail?.AdjustedWeighingSupplierName ?? string.Empty,
            AdjustedWeighingHalfCarcassCount = stockDetail?.AdjustedWeighingHalfCarcassCount,
            AdjustedWeighingWeightKg = stockDetail?.AdjustedWeighingWeightKg,
            CreatedAtLabel = stockDetail?.CreatedAtLabel ?? string.Empty,
            CreatedByLabel = stockDetail?.CreatedByLabel ?? string.Empty,
            UpdatedAtLabel = stockDetail?.UpdatedAtLabel ?? string.Empty,
            UpdatedByLabel = stockDetail?.UpdatedByLabel ?? string.Empty,
            Lines = stockDetail?.Lines.Select(MapLine).ToList() ?? [],
            ItemCount = stockDetail?.Lines.Count ?? 0,
            TotalQuantityKg = stockDetail?.Lines.Sum(x => x.QuantityKg) ?? 0,
            Branches = branches,
            OperationTypes = StockEditViewModel.DefaultOperationTypes,
            QuickProducts = quickProducts
        };

        return View(model);
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(StockEditViewModel model, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model.StockOperationType = NormalizeOperationType(model.StockOperationType);
        model.Notes = (model.Notes ?? string.Empty).Trim();
        model.SupplierName = (model.SupplierName ?? string.Empty).Trim();
        model.SupplierTaxId = (model.SupplierTaxId ?? string.Empty).Trim();
        model.Lines ??= [];

        if (string.IsNullOrWhiteSpace(model.StockOperationType))
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar una accion valida.");
        }

        if (model.BranchId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Debe seleccionar una sucursal valida.");
        }

        if (model.Lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Debe agregar al menos una linea al movimiento.");
        }

        if (model.Lines.Any(x => !x.ProductId.HasValue || x.ProductId.Value <= 0))
        {
            ModelState.AddModelError(string.Empty, "Todas las lineas deben tener un producto valido.");
        }

        if (!string.Equals(model.StockOperationType, "Cierre Stock", StringComparison.OrdinalIgnoreCase)
            && model.Lines.Any(x => x.QuantityKg == 0))
        {
            ModelState.AddModelError(string.Empty, "Todas las lineas deben tener una cantidad distinta de cero.");
        }

        if (!permissionService.CanEdit(
                currentUser,
                "formAddOrEditStock",
                DateOnly.FromDateTime(model.OperationDate),
                currentUser.UserId))
        {
            ModelState.AddModelError(string.Empty, "No tiene permisos para guardar este movimiento en la fecha indicada.");
        }

        if (!ModelState.IsValid)
        {
            await ReloadEditListsAsync(model, currentUser.Company.CompanyId, cancellationToken);
            RecalculateTotals(model);
            return View("Edit", model);
        }

        var result = await stockCommandService.SaveStockAsync(
            new StockSaveRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                UserId = currentUser.UserId,
                StockId = model.StockId,
                StockOperationType = model.StockOperationType,
                BranchId = model.BranchId,
                OperationDate = model.OperationDate,
                Notes = model.Notes,
                SupplierId = model.SupplierId,
                HalfCarcassCount = model.HalfCarcassCount,
                HalfCarcassWeightKg = model.HalfCarcassWeightKg,
                SaveWithoutWeighing = model.SaveWithoutWeighing,
                LinkedWeighingId = model.LinkedWeighingId,
                Lines = model.Lines
                    .Where(x => x.ProductId.HasValue && x.ProductId.Value > 0)
                    .Select(x => new StockSaveLineRequest
                    {
                        ProductId = x.ProductId!.Value,
                        QuantityKg = x.QuantityKg,
                        ScaleWeight = x.ScaleWeight
                    })
                    .ToArray()
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            await ReloadEditListsAsync(model, currentUser.Company.CompanyId, cancellationToken);
            RecalculateTotals(model);
            return View("Edit", model);
        }

        TempData["FlashSuccess"] = model.IsEdit
            ? "El movimiento de stock se guardo correctamente."
            : "El movimiento de stock se registro correctamente.";

        return RedirectToAction(nameof(Edit), new { id = result.StockId, tipoCompra = model.StockOperationType });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> WeighingPurchases(
        int branchId = 0,
        int currentStockId = 0,
        string supplier = "",
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool onlyWeighings = false,
        CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida.", items = Array.Empty<object>() });
        }

        if (branchId <= 0)
        {
            return Json(new { ok = false, message = "Seleccione una sucursal valida.", items = Array.Empty<object>() });
        }

        var items = await stockQueryService.GetWeighingPurchasesAsync(
            currentUser.Company.CompanyId,
            new StockWeighingPurchaseQuery
            {
                BranchId = branchId,
                CurrentStockId = currentStockId,
                SupplierSearchText = supplier ?? string.Empty,
                FromDate = fromDate,
                ToDate = toDate,
                OnlyWeighings = onlyWeighings
            },
            cancellationToken);

        return Json(new
        {
            ok = true,
            items = items.Select(x => new
            {
                stockId = x.StockId,
                supplierId = x.SupplierId,
                supplierName = x.SupplierName,
                supplierTaxId = x.SupplierTaxId,
                operationDate = x.OperationDate.ToString("dd/MM/yyyy HH:mm"),
                stockOperationType = x.StockOperationType,
                halfCarcassCount = x.HalfCarcassCount ?? 0,
                halfCarcassWeightKg = x.HalfCarcassWeightKg ?? x.TotalQuantityKg,
                totalQuantityKg = x.TotalQuantityKg
            })
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> WeighingDetailLines(int stockId, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida." });
        }

        if (stockId <= 0)
        {
            return Json(new { ok = false, message = "Seleccione un pesaje valido." });
        }

        var item = await stockQueryService.GetStockEditByIdAsync(
            currentUser.Company.CompanyId,
            stockId,
            cancellationToken);

        if (item is null || !string.Equals(item.StockOperationType, "Pesaje Cortes", StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { ok = false, message = "No se encontro el pesaje seleccionado." });
        }

        return Json(new
        {
            ok = true,
            item = new
            {
                stockId = item.StockId,
                supplierId = item.SupplierId,
                supplierName = item.SupplierName,
                supplierTaxId = item.SupplierTaxId,
                operationDate = item.OperationDate.ToString("dd/MM/yyyy HH:mm"),
                stockOperationType = item.StockOperationType,
                lines = item.Lines.Select(x => new
                {
                    productId = x.ProductId,
                    code = x.Code,
                    productName = x.ProductName,
                    quantityKg = x.QuantityKg,
                    scaleWeight = x.ScaleWeight,
                    isWeighable = x.IsWeighable
                })
            }
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> WeighingPurchaseDetail(int stockId, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida." });
        }

        if (stockId <= 0)
        {
            return Json(new { ok = false, message = "Seleccione una compra valida." });
        }

        var item = await stockQueryService.GetWeighingPurchaseByIdAsync(
            currentUser.Company.CompanyId,
            stockId,
            cancellationToken);

        if (item is null)
        {
            return Json(new { ok = false, message = "No se encontro la compra seleccionada." });
        }

        return Json(new
        {
            ok = true,
            item = new
            {
                stockId = item.StockId,
                supplierId = item.SupplierId,
                supplierName = item.SupplierName,
                supplierTaxId = item.SupplierTaxId,
                operationDate = item.OperationDate.ToString("dd/MM/yyyy HH:mm"),
                stockOperationType = item.StockOperationType,
                halfCarcassCount = item.HalfCarcassCount ?? 0,
                halfCarcassWeightKg = item.HalfCarcassWeightKg ?? item.TotalQuantityKg,
                totalQuantityKg = item.TotalQuantityKg
            }
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
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

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpGet]
    public async Task<IActionResult> ProductLookup(string searchText = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { items = Array.Empty<object>() });
        }

        var items = await movementProductLookupService.GetQuickProductsAsync(
            currentUser.Company.CompanyId,
            int.MaxValue,
            80,
            cancellationToken);

        var normalizedSearch = (searchText ?? string.Empty).Trim();
        var filtered = string.IsNullOrWhiteSpace(normalizedSearch)
            ? items
            : items.Where(x =>
                    x.Description.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)
                    || x.Code.ToString(CultureInfo.InvariantCulture).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                .ToArray();

        return Json(new
        {
            items = filtered
                .Take(80)
                .Select(x => new
                {
                    productId = x.ProductId,
                    code = x.Code,
                    description = x.Description,
                    type = x.Type,
                    weighable = x.Weighable,
                    averageWeight = x.AverageWeight,
                    pricePerKilogramText = "0.00"
                })
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpPost]
    public async Task<IActionResult> WeighingPercentages([FromForm] StockWeighingPercentagesRequest? request, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida." });
        }

        var stockId = request?.WeighingId ?? 0;
        if (stockId <= 0)
        {
            return Json(new { ok = false, message = "Guarde el pesaje y vuelva a intentarlo." });
        }

        var analysis = await stockQueryService.GetWeighingPercentagesAsync(
            currentUser.Company.CompanyId,
            stockId,
            cancellationToken);

        if (analysis is null)
        {
            return Json(new { ok = false, message = "No se encontro el pesaje seleccionado." });
        }

        if (!analysis.HasRequiredMediaData)
        {
            return Json(new
            {
                ok = false,
                message = "El pesaje no tiene registrado KgsMedias y CantMedias. Ingrese KgsMedias y CantMedias, presione Guardar y vuelva a intentarlo."
            });
        }

        return Json(new
        {
            ok = true,
            estado = analysis.Status,
            promMedias = new
            {
                columnas = analysis.AverageHalfCarcassesTable.Columns.Select(x => new
                {
                    nombre = x.Name,
                    oculta = x.Hidden,
                    alineacionDerecha = x.RightAligned,
                    formatoTresDecimales = x.ThreeDecimalFormat
                }),
                filas = analysis.AverageHalfCarcassesTable.Rows
            },
            porcCortes = new
            {
                columnas = analysis.CutPercentagesTable.Columns.Select(x => new
                {
                    nombre = x.Name,
                    oculta = x.Hidden,
                    alineacionDerecha = x.RightAligned,
                    formatoTresDecimales = x.ThreeDecimalFormat
                }),
                filas = analysis.CutPercentagesTable.Rows
            }
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateWeighingAdjustment([FromForm] StockWeighingPercentagesRequest? request, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida." });
        }

        var stockId = request?.WeighingId ?? 0;
        if (stockId <= 0)
        {
            return Json(new { ok = false, message = "Guarde el pesaje y vuelva a intentarlo." });
        }

        var result = await stockCommandService.GenerateWeighingAdjustmentAsync(
            new StockGenerateWeighingAdjustmentRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                UserId = currentUser.UserId,
                WeighingId = stockId
            },
            cancellationToken);

        if (!result.Success)
        {
            return Json(new { ok = false, message = result.ErrorMessage });
        }

        return Json(new
        {
            ok = true,
            message = "El Ajuste de Stock se realizo correctamente.",
            estado = result.Status,
            adjustmentId = result.AdjustmentId
        });
    }

    [RequirePermission("formAddOrEditStock", PermissionMode.Edit)]
    [HttpPost]
    public async Task<IActionResult> MissingClosingProducts([FromBody] MissingClosingProductsRequest? request, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, message = "Sesion invalida.", items = Array.Empty<object>() });
        }

        request ??= new MissingClosingProductsRequest();
        if (request.BranchId <= 0)
        {
            return Json(new { ok = false, message = "Seleccione una sucursal valida.", items = Array.Empty<object>() });
        }

        var items = await stockQueryService.GetMissingClosingProductsAsync(
            currentUser.Company.CompanyId,
            request.BranchId,
            request.OperationDate ?? DateTime.Now,
            request.LoadedCodes ?? Array.Empty<long>(),
            cancellationToken);

        return Json(new
        {
            ok = true,
            items = items.Select(x => new
            {
                productId = x.ProductId,
                code = x.Code,
                description = x.ProductName,
                currentStock = x.CurrentStock,
                currentStockText = x.CurrentStock.ToString("0.000", CultureInfo.InvariantCulture),
                weighable = x.Weighable,
                averageWeight = x.AverageWeight
            })
        });
    }

    private static string NormalizeOperationType(string? tipoCompra)
    {
        var normalized = (tipoCompra ?? string.Empty).Trim();
        return StockEditViewModel.DefaultOperationTypes
            .FirstOrDefault(x => string.Equals(x, normalized, StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;
    }

    private static StockEditLineViewModel MapLine(StockEditLineItem item)
    {
        return new StockEditLineViewModel
        {
            Index = item.Index,
            ProductId = item.ProductId,
            Code = item.Code,
            ProductName = item.ProductName,
            QuantityKg = item.QuantityKg,
            ScaleWeight = item.ScaleWeight,
            CreatedLabel = item.CreatedLabel,
            IsWeighable = item.IsWeighable,
            SourceType = string.Empty,
            SourceStockId = null,
            SourceLabel = string.Empty
        };
    }

    private async Task ReloadEditListsAsync(StockEditViewModel model, int companyId, CancellationToken cancellationToken)
    {
        model.Branches = await branchLookupService.GetBranchesAsync(companyId, cancellationToken);
        model.OperationTypes = StockEditViewModel.DefaultOperationTypes;
        model.QuickProducts = (await movementProductLookupService.GetQuickProductsAsync(companyId, 1000, 1000, cancellationToken)).ToList();
        model.BranchName = model.Branches.FirstOrDefault(x => x.BranchId == model.BranchId)?.BranchName ?? model.BranchName;
    }

    private static void RecalculateTotals(StockEditViewModel model)
    {
        model.ItemCount = model.Lines?.Count ?? 0;
        model.TotalQuantityKg = model.Lines?.Sum(x => x.QuantityKg) ?? 0;
    }

    public sealed class MissingClosingProductsRequest
    {
        public int BranchId { get; set; }

        public DateTime? OperationDate { get; set; }

        public long[] LoadedCodes { get; set; } = Array.Empty<long>();
    }
}
