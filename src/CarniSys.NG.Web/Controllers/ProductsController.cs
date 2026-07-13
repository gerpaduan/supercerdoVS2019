using CarniSys.NG.Application.Products;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class ProductsController(
    IUserSessionAccessor userSessionAccessor,
    IProductQueryService productQueryService) : Controller
{
    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", string type = "", CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Challenge();
        }

        var items = await productQueryService.GetProductsAsync(
            currentUser.Company.CompanyId,
            new ProductListQuery
            {
                SearchText = searchText ?? string.Empty,
                Type = type ?? string.Empty
            },
            cancellationToken);

        var model = new ProductListPageViewModel
        {
            SearchText = searchText ?? string.Empty,
            Type = type ?? string.Empty,
            Items = items
        };

        return View(model);
    }

    [RequirePermission("formCortes", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Lookup(string searchText = "", string type = "", int skip = 0, int take = 50, CancellationToken cancellationToken = default)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            return Json(new { items = Array.Empty<object>() });
        }

        var items = await productQueryService.GetProductsAsync(
            currentUser.Company.CompanyId,
            new ProductListQuery
            {
                SearchText = searchText ?? string.Empty,
                Type = type ?? string.Empty,
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
                    productId = x.ProductId,
                    code = x.Code,
                    description = x.Description,
                    type = x.Type,
                    weighable = x.Weighable,
                    pricePerKilogram = x.PricePerKilogram,
                    pricePerKilogramText = x.PricePerKilogram.ToString("0.00", CultureInfo.InvariantCulture)
                })
        });
    }
}
