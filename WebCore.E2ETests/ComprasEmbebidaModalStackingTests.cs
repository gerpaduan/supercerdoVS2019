using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario 2026-09-12 (ver docs/DECISIONS.md "Batch 3: Compras embebida
// en POS -- causa raiz mas profunda"): el fix anterior (elevarZIndexSobreModalAbierto, solo
// z-index) no alcanzaba porque #modalBuscarPersona se autocura como DESCENDIENTE del DOM de
// #modalFinanzasPOS (no un sibling real) -- necesitaba 2 intentos de F9 para mostrarse
// correctamente, y Escape cerraba en cascada todo el modal de Compras. #modalBuscarProducto (SI
// es un sibling real desde el arranque) tenia un problema distinto: el FocusTrap de
// #modalFinanzasPOS le disputaba el foco, tambien necesitando 2 intentos de F10 y sin devolver el
// foco a #txtCantKgs tras seleccionar un producto. Fix: re-parentar #modalBuscarPersona a
// document.body cuando se autocura embebido en POS (compras.js, abrirProveedorModal), y
// desactivar/reactivar el FocusTrap de #modalFinanzasPOS alrededor del ciclo de vida de ambos
// modales anidados (gestionarFocusTrapModalAbierto).
[Collection("WebCore browser")]
public sealed class ComprasEmbebidaModalStackingTests
{
    private readonly WebCoreFixture _fixture;

    public ComprasEmbebidaModalStackingTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirNuevaCompraEnPOSAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F5");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(600);
        return page;
    }

    [Fact]
    public async Task BuscarProveedor_F9_AbreVisibleAlPrimerIntentoYEscapeNoCierraCompras()
    {
        var page = await AbrirNuevaCompraEnPOSAsync(_fixture);

        await page.Keyboard.PressAsync("F9");
        await page.Locator("#modalBuscarPersona.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        // El bug real: aunque tuviera la clase "show", quedaba invisible/no clickeable detras de
        // #modalFinanzasPOS (mismo z-index, gana el ultimo en el DOM) -- se verifica visibilidad
        // real, no solo la clase.
        Assert.True(await page.Locator("#modalBuscarPersona").IsVisibleAsync(), "el buscador de proveedor deberia ser visible al primer F9, no requerir un segundo intento");
        Assert.True(await page.Locator("#filtroPersona").IsVisibleAsync());

        // Confirma que quedo re-parentado como sibling de body -- ya no descendiente de
        // #modalFinanzasPOS (causa raiz del bug de z-index/escape en cascada).
        var esDescendienteDeFinanzas = await page.EvaluateAsync<bool>(
            "() => { var f = document.getElementById('modalFinanzasPOS'); var p = document.getElementById('modalBuscarPersona'); return !!(f && p && f.contains(p)); }");
        Assert.False(esDescendienteDeFinanzas, "el buscador de proveedor deberia haberse re-parentado a body, no seguir anidado dentro de #modalFinanzasPOS");

        // Escape solo debe cerrar el buscador de proveedor, NO todo el modal de Compras.
        await page.Keyboard.PressAsync("Escape");
        await page.Locator("#modalBuscarPersona.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task BuscarProducto_F10_AbreVisibleYFocoQuedaEnCantidadTrasSeleccionar()
    {
        var page = await AbrirNuevaCompraEnPOSAsync(_fixture);

        await page.Keyboard.PressAsync("F10");
        await page.Locator("#modalBuscarProducto.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        Assert.True(await page.Locator("#modalBuscarProducto").IsVisibleAsync(), "el buscador de producto deberia ser visible al primer F10, no requerir un segundo intento");

        var fila = page.Locator("#modalBuscarProducto .js-buscar-producto-row").First;
        Assert.True(await fila.CountAsync() > 0, "hace falta al menos un producto real en la base de dev para este test");
        await fila.ClickAsync();
        await page.WaitForTimeoutAsync(300);

        // Simula el flujo real: click en la fila deja is-selected, doble click o Enter confirma.
        // seleccionarProductoDesdeModal se dispara con dblclick en la fila (mismo patron que
        // proveedor) -- ver compras.js.
        await fila.DblClickAsync();
        await page.Locator("#modalBuscarProducto.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        var idFocoActual = await page.EvaluateAsync<string>("() => document.activeElement ? document.activeElement.id : ''");
        Assert.Equal("txtCantKgs", idFocoActual);

        await page.CloseAsync();
    }
}
