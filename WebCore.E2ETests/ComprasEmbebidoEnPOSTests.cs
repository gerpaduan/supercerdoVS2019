using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal "Nueva Compra" embebido en POS (item 5 de la cuarta ronda de pedidos, 2026-09-10, ver
// docs/DECISIONS.md "Batch 3: modal Nueva Compra embebido en POS"). Bug real: Compras/Editar.cshtml
// no nuleaba su Layout para el caso DesdePos=true, asi que la pagina completa (con _Layout.cshtml:
// nav, sidebar, sus propios scripts) quedaba anidada dentro del modal chico de POS -- ademas
// modalProductoSelector apuntaba a un modal nested que nunca se renderizaba en ese caso, dejando
// "Buscar producto"/F10 muerto en silencio. Restaurado el branch condicional que clasico ya tenia
// (Web/Views/Compras/Editar.cshtml:498-548).
[Collection("WebCore browser")]
public sealed class ComprasEmbebidoEnPOSTests
{
    private readonly WebCoreFixture _fixture;

    public ComprasEmbebidoEnPOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirNuevaCompraDesdePOSAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F5");
        await page.Locator("#formCompra").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);
        return page;
    }

    [Fact]
    public async Task NuevaCompraDesdePOS_NoQuedaConNavDuplicadoYFormularioUnico()
    {
        var page = await AbrirNuevaCompraDesdePOSAsync(_fixture);
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        // Bug real antes del fix: se anidaba la pagina completa (con su propio _Layout.cshtml:
        // nav/sidebar/scripts) dentro del modal chico de POS -- 1 solo formCompra en el DOM y
        // ningun jquery/bootstrap script duplicado son la prueba mecanica de que eso no pasa mas.
        Assert.Equal(1, await page.Locator("#formCompra").CountAsync());
        var scriptsJQuery = await page.EvaluateAsync<int>(
            "() => Array.from(document.querySelectorAll('script[src]')).filter(s => s.src.indexOf('jquery.min.js') >= 0).length");
        Assert.Equal(1, scriptsJQuery);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task NuevaCompraDesdePOS_F9BuscarProveedor_AbreYSeleccionaProveedorReal()
    {
        var page = await AbrirNuevaCompraDesdePOSAsync(_fixture);

        await page.Keyboard.PressAsync("F9");
        await page.Locator("#modalBuscarPersona.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        await page.FillAsync("#filtroPersona", "a");
        await page.WaitForTimeoutAsync(600);
        Assert.True(await page.Locator("#tablaPersonas tr.fila-persona").CountAsync() > 0);

        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(0, await page.Locator("#modalBuscarPersona.show").CountAsync());
        Assert.NotEqual("", (await page.InputValueAsync("#razonSocial")).Trim());

        await page.CloseAsync();
    }

    [Fact]
    public async Task NuevaCompraDesdePOS_F10BuscarProducto_AbreElModalGenericoDePOS()
    {
        var page = await AbrirNuevaCompraDesdePOSAsync(_fixture);

        // Antes del fix, esto no hacia NADA (silencioso): modalProductoSelector apuntaba a
        // "#modalBuscarProductoCompra", que nunca se renderiza cuando DesdePos=true (linea 475 de
        // Compras/Editar.cshtml). Ahora reusa "#modalBuscarProducto", el modal generico YA
        // presente en la pagina padre de POS (Ventas/POS.cshtml:744).
        await page.Keyboard.PressAsync("F10");
        await page.Locator("#modalBuscarProducto.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        await page.CloseAsync();
    }

    [Fact]
    public async Task NavegacionNormalDeCompras_SigueFuncionandoIgualQueAntes()
    {
        // Regresion: /Compras/NuevaCompra (origen=layout, fuera de POS) debe seguir con el
        // comportamiento de siempre -- layout completo, modalProductoSelector apuntando al modal
        // nested propio de esta vista.
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras/NuevaCompra", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(1, await page.Locator("#formCompra").CountAsync());
        Assert.Equal(1, await page.Locator("#accordionSidebar").CountAsync());

        await page.CloseAsync();
    }
}
