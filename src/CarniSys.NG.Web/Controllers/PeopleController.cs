using CarniSys.NG.Application.People;
using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class PeopleController(
    IUserSessionAccessor userSessionAccessor,
    IPersonQueryService personQueryService,
    IPersonCommandService personCommandService,
    IAfipPadronLookupService afipPadronLookupService,
    IPermissionService permissionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await personQueryService.GetPeopleAsync(
            currentUser.Company.CompanyId,
            new PersonListQuery
            {
                SearchText = searchText ?? string.Empty
            },
            cancellationToken);

        var model = new PersonListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            Items = items
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Lookup(string searchText = "", int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { items = Array.Empty<object>() });
        }

        var items = await personQueryService.GetPeopleAsync(
            currentUser.Company.CompanyId,
            new PersonListQuery
            {
                SearchText = searchText ?? string.Empty,
                Skip = skip,
                Take = take
            },
            cancellationToken);

        return Json(new
        {
            hasMore = items.Count >= Math.Min(take <= 0 ? 50 : take, 100),
            items = items
                .Select(x => new
                {
                    personId = x.PersonId,
                    identification = x.Identification,
                    businessName = x.BusinessName,
                    taxId = x.TaxId
                })
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

        var item = await personQueryService.GetPersonByIdAsync(id, cancellationToken);
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

    [HttpGet]
    public async Task<IActionResult> Edit(int id = 0, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        PersonEditViewModel model;
        if (id > 0)
        {
            var item = await personQueryService.GetPersonByIdAsync(id, cancellationToken);
            if (item is null)
            {
                return NotFound();
            }

            if (item.CompanyId != 0 && item.CompanyId != currentUser.Company.CompanyId)
            {
                return Forbid();
            }

            if (!CanModifyPerson(currentUser.Company.CompanyId, item.CompanyId))
            {
                TempData["FlashError"] = "No tiene permisos para modificar personas globales.";
                return RedirectToAction(nameof(Index));
            }

            var hasMovements = await PersonHasMovementsAsync(item.PersonId, cancellationToken);
            var canManageCurrentAccount = CanManageCurrentAccount(currentUser);

            model = new PersonEditViewModel
            {
                PersonId = item.PersonId,
                IsEdit = true,
                IsInitiallyReadOnly = true,
                HasMovements = hasMovements,
                IsAdministrator = currentUser.IsAdministrator,
                CanManageCurrentAccount = canManageCurrentAccount,
                CanEditProtectedFields = !hasMovements || currentUser.IsAdministrator,
                RestrictionMessage = BuildRestrictionMessage(hasMovements, currentUser.IsAdministrator),
                Identification = item.Identification,
                BusinessName = item.BusinessName,
                VatId = item.VatId > 0 ? item.VatId : null,
                TaxId = item.TaxId,
                Phone = item.Phone,
                Email = item.Email,
                Address = item.Address,
                City = item.City,
                Notes = item.Notes,
                HasCurrentAccount = item.HasCurrentAccount,
                DiscountText = item.Discount.ToString("0.##", CultureInfo.InvariantCulture)
            };
        }
        else
        {
            model = new PersonEditViewModel
            {
                IsAdministrator = currentUser.IsAdministrator,
                CanManageCurrentAccount = CanManageCurrentAccount(currentUser),
                CanEditProtectedFields = true
            };
        }

        await LoadVatOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PersonEditViewModel model, CancellationToken cancellationToken)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        model ??= new PersonEditViewModel();
        NormalizeModel(model);

        var existingPerson = model.PersonId > 0
            ? await personQueryService.GetPersonByIdAsync(model.PersonId, cancellationToken)
            : null;

        if (model.PersonId > 0 && existingPerson is null)
        {
            TempData["FlashError"] = "No se encontro la persona a modificar.";
            return RedirectToAction(nameof(Index));
        }

        if (existingPerson is not null)
        {
            if (existingPerson.CompanyId != 0 && existingPerson.CompanyId != currentUser.Company.CompanyId)
            {
                return Forbid();
            }

            if (!CanModifyPerson(currentUser.Company.CompanyId, existingPerson.CompanyId))
            {
                TempData["FlashError"] = "No tiene permisos para modificar personas globales.";
                return RedirectToAction(nameof(Index));
            }
        }

        var hasMovements = existingPerson is not null && await PersonHasMovementsAsync(existingPerson.PersonId, cancellationToken);
        model.IsEdit = model.PersonId > 0;
        model.IsInitiallyReadOnly = false;
        model.HasMovements = hasMovements;
        model.IsAdministrator = currentUser.IsAdministrator;
        model.CanManageCurrentAccount = CanManageCurrentAccount(currentUser);
        model.CanEditProtectedFields = !hasMovements || currentUser.IsAdministrator;
        model.RestrictionMessage = BuildRestrictionMessage(hasMovements, currentUser.IsAdministrator);

        if (!TryParseDiscount(model.DiscountText, out var discount))
        {
            ModelState.AddModelError(nameof(model.DiscountText), "La bonificacion debe ser numerica.");
        }

        if (!ModelState.IsValid)
        {
            await LoadVatOptionsAsync(model, cancellationToken);
            return View(model);
        }

        var result = await personCommandService.SavePersonAsync(
            new PersonSaveRequest
            {
                CompanyId = currentUser.Company.CompanyId,
                PersonId = model.PersonId,
                Identification = model.Identification,
                BusinessName = model.BusinessName,
                VatId = model.VatId ?? 0,
                TaxId = model.TaxId,
                Phone = model.Phone,
                Email = model.Email,
                Address = model.Address,
                City = model.City,
                Notes = model.Notes,
                HasCurrentAccount = model.HasCurrentAccount,
                Discount = discount,
                IsAdministrator = currentUser.IsAdministrator,
                CanManageCurrentAccount = model.CanManageCurrentAccount
            },
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage);
            await LoadVatOptionsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["FlashSuccess"] = model.IsEdit
            ? "La persona se guardo correctamente."
            : "La persona se creo correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> AfipLookup(string taxId, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { ok = false, msg = "Sesion invalida.", tipo = "error" });
        }

        var result = await afipPadronLookupService.LookupAsync(
            currentUser.Company.CompanyId,
            taxId,
            cancellationToken);

        return Json(new
        {
            ok = result.Success,
            razonSocial = result.BusinessName,
            identificacion = result.Identification,
            domicilio = result.Address,
            ciudad = result.City,
            idIva = result.SuggestedVatId,
            condicionIva = result.VatCondition,
            estadoClave = result.TaxStatus,
            actividadPrincipal = result.MainActivity,
            msg = result.Message,
            tipo = result.MessageType
        });
    }

    private async Task<bool> PersonHasMovementsAsync(int personId, CancellationToken cancellationToken)
    {
        return await personQueryService.HasSalesOrPurchasesAsync(personId, cancellationToken);
    }

    private bool CanManageCurrentAccount(CarniSys.NG.Domain.Authentication.AuthenticatedUser currentUser)
    {
        return currentUser.IsAdministrator ||
               permissionService.CanRead(currentUser, "formCtasCtes", DateOnly.FromDateTime(DateTime.Today));
    }

    private static bool CanModifyPerson(int currentCompanyId, int personCompanyId)
    {
        return !(currentCompanyId > 0 && personCompanyId == 0);
    }

    private static string BuildRestrictionMessage(bool hasMovements, bool isAdministrator)
    {
        if (!hasMovements)
        {
            return string.Empty;
        }

        return isAdministrator
            ? "Esta persona ya tiene compras o ventas registradas. Revise con cuidado los cambios en Razon Social, CUIT e Identificacion porque impactan en datos historicos."
            : "Esta persona ya tiene compras o ventas registradas. Por seguridad, solo un administrador puede modificar Razon Social, CUIT o Identificacion.";
    }

    private async Task LoadVatOptionsAsync(PersonEditViewModel model, CancellationToken cancellationToken)
    {
        var vatOptions = await personQueryService.GetVatOptionsAsync(cancellationToken);
        model.VatOptions = vatOptions
            .Select(x => new SelectListItem
            {
                Value = x.VatId.ToString(CultureInfo.InvariantCulture),
                Text = x.Label,
                Selected = x.VatId == (model.VatId ?? 0)
            })
            .ToList();
    }

    private static void NormalizeModel(PersonEditViewModel model)
    {
        model.Identification = (model.Identification ?? string.Empty).Trim();
        model.BusinessName = (model.BusinessName ?? string.Empty).Trim();
        model.TaxId = (model.TaxId ?? string.Empty).Trim();
        model.Phone = (model.Phone ?? string.Empty).Trim();
        model.Email = (model.Email ?? string.Empty).Trim();
        model.Address = (model.Address ?? string.Empty).Trim();
        model.City = (model.City ?? string.Empty).Trim();
        model.Notes = (model.Notes ?? string.Empty).Trim();
        model.DiscountText = string.IsNullOrWhiteSpace(model.DiscountText) ? "0" : model.DiscountText.Trim();
    }

    private static bool TryParseDiscount(string? text, out decimal value)
    {
        value = 0m;
        var normalized = (text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return true;
        }

        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out value)
            || decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }
}
