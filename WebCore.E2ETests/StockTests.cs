using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario 2026-09-11 (ver docs/DECISIONS.md): un alta/edicion de Stock
// se guardaba correctamente, pero al volver al listado (Guardar redirige a Index sin querystring,
// patron POST/Redirect/GET estandar) el registro recien creado no aparecia. Causa raiz:
// StockController.Index() tenia un default de sucursal HARDCODEADO a 2 (San Lorenzo) desde la
// epoca del stub sin sesion real -- nunca actualizado cuando se porto el login real (2026-09-06).
// Con "ger" logueado en una sucursal distinta (San Martin, id=1), el alta se guardaba con la
// sucursal REAL del usuario (CrearViewModelNuevo ya usaba user.IdSucursal correctamente) pero el
// Index, al volver sin ?idSucursal en la URL, filtraba por la sucursal equivocada. Fix:
// StockController.cs, Index() usa _usuarioActual.IdSucursal como default (mismo criterio que
// Lineas(), que ya lo hacia bien).
[Collection("WebCore browser")]
public sealed class StockTests
{
    private readonly WebCoreFixture _fixture;

    public StockTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NuevoIngresoStock_GuardaYApareceEnElIndexSinFiltroExplicito()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        // Mismo quirk heredado que MovimientosTests (setSelectionRange sobre type="number") --
        // presente identico en Web clasico, fuera de alcance de esta migracion (CLAUDE.md §5).
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange")) errors.Add(msg);
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo?tipoCompra=Ingreso Stock", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Locator("#txtCodigoProducto").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtProductoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        await page.FillAsync("#txtCantKgs", "1");
        await page.ClickAsync("#btnAgregarLineaStock");
        await page.WaitForSelectorAsync("#tablaLineasStock tbody tr", new PageWaitForSelectorOptions { Timeout = 5000 });
        Assert.Equal(1, await page.Locator("#tablaLineasStock tbody tr").CountAsync());

        var marcador = "E2E stock " + DateTime.UtcNow.Ticks;
        await page.FillAsync("#Observaciones", marcador);

        await page.ClickAsync("#btnGuardarStock");
        await page.WaitForURLAsync(u => u.Contains("/Stock") && !u.Contains("/Stock/Editar") && !u.Contains("/Stock/Nuevo"), new PageWaitForURLOptions { Timeout = 15000 });

        // El bug real: Guardar redirige a Index SIN querystring de sucursal -- si el default de
        // ese filtro no coincide con la sucursal real donde se guardo el registro, no aparece.
        Assert.DoesNotContain("idSucursal=", page.Url);

        await page.WaitForTimeoutAsync(500);
        Assert.True(await page.Locator("#tablaLineasStock, .stock-index-page table tbody tr").CountAsync() > 0,
            "el listado de Stock deberia mostrar al menos una fila tras guardar, con el filtro de sucursal por defecto");

        // Assert principal: el badge de observaciones de ALGUNA fila trae nuestro marcador -- prueba
        // que el registro recien guardado esta realmente visible en este Index sin filtro explicito,
        // no solo que "hay filas" (que podrian ser de otro registro viejo).
        var badges = page.Locator(".badge-pill.badge-info[title]");
        int totalBadges = await badges.CountAsync();
        bool encontrado = false;
        for (int i = 0; i < totalBadges; i++)
        {
            var title = await badges.Nth(i).GetAttributeAsync("title");
            if (title != null && title.Contains(marcador)) { encontrado = true; break; }
        }
        Assert.True(encontrado, "el registro de Stock recien guardado (marcador en Observaciones) deberia aparecer en el Index sin pasar ?idSucursal explicito");

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
