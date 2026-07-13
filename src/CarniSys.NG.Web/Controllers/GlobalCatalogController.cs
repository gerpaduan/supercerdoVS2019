using CarniSys.NG.Application.Products;
using CarniSys.NG.Web.Authorization;
using CarniSys.NG.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Controllers;

[Authorize]
public class GlobalCatalogController(IProductQueryService productQueryService) : Controller
{
    [RequirePermission("formNuevoCorte", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Index(string searchText = "", string type = "", CancellationToken cancellationToken = default)
    {
        var items = await productQueryService.GetGlobalProductsAsync(
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

    [RequirePermission("formNuevoCorte", PermissionMode.Read)]
    [HttpGet]
    public async Task<IActionResult> Detail(int id, CancellationToken cancellationToken = default)
    {
        var item = await productQueryService.GetGlobalProductByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        var model = new ProductDetailPageViewModel
        {
            Item = item,
            CanEdit = false
        };

        return View(model);
    }
}
