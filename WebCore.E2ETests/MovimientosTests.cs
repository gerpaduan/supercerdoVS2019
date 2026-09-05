using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modulo Movimientos (traslados de stock entre sucursales), no incluido en el plan original de
// migracion en 8 modulos -- se ataco en una sesion aparte (ver docs/10-migracion-aspnet-core/
// README.md). Prueba real de punta a punta: alta de un movimiento nuevo con click + submit real
// (antiforgery real, sin bypass), igual que GenerarEtiquetasPdfTests para Productos.
[Collection("WebCore browser")]
public sealed class MovimientosTests
{
    private readonly WebCoreFixture _fixture;

    public MovimientosTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NuevoMovimiento_AgregaLineaYGuardaReal()
    {
        var page = await _fixture.Browser.NewPageAsync();
        var errors = new List<string>();
        // Filtra en origen el quirk heredado del original (Web/Views/Movimientos/Editar.cshtml
        // usa el mismo type="number" en #txtCodigoProducto/#txtCantUnidad): movimientos.js llama
        // a setSelectionRange/.select() sobre esos inputs, y los navegadores no lo permiten para
        // type=number -- error real en consola, presente identico en Web clasico (mismo JS,
        // mismo markup, dispara varias veces durante el flujo normal). No se corrige aca
        // (CLAUDE.md §5, fuera de alcance de esta migracion).
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange")) errors.Add(msg);
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        // Codigo real (CARRE, ya usado en el resto de la migracion) -- Enter dispara
        // BuscarProductoPorCodigo y completa el nombre del producto.
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Locator("#txtCodigoProducto").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtProductoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        var nombreProducto = await page.InputValueAsync("#txtProductoNombre");
        Assert.False(string.IsNullOrWhiteSpace(nombreProducto));

        await page.FillAsync("#txtCantUnidad", "1");
        await page.FillAsync("#txtCantKgs", "1,5");
        await page.ClickAsync("#btnAgregarProducto");

        // La linea se agrega a la tabla client-side (mismo mecanismo que el resto de esta
        // migracion para tablas de lineas: Stock/Compras/Ventas).
        await page.WaitForSelectorAsync("#tablaLineasMovimiento tbody tr", new PageWaitForSelectorOptions { Timeout = 5000 });
        Assert.Equal(1, await page.Locator("#tablaLineasMovimiento tbody tr").CountAsync());

        var marcador = "E2E movimiento " + DateTime.UtcNow.Ticks;
        await page.FillAsync("#Observaciones", marcador);

        await page.ClickAsync("#btnGuardarMovimiento");

        // Guardado real -> se abre el modal post-guardado (version reducida, sin ticket ESC/POS).
        await page.WaitForSelectorAsync("#modalPostMovimiento.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.Equal(1, await page.Locator("#modalPostMovimiento:visible").CountAsync());

        Assert.Empty(errors);

        // Cierra sin navegar (no hace falta limpiar: Movimientos no tiene accion de anulacion/borrado
        // en el original -- mismo criterio que las compras/ventas de prueba de este programa, quedan
        // marcadas en Observaciones, no se borran).
        await page.ClickAsync("#btnPostMovimientoNoImprimir");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/Movimientos", page.Url);

        await page.CloseAsync();
    }
}
