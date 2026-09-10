using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modulo Elaborados (catalogo de embutidos: carga manual, ingreso rapido con formula, formulas),
// no incluido en el plan original de migracion en 8 modulos -- se ataco en la misma sesion que
// Movimientos (ver docs/10-migracion-aspnet-core/README.md). Prueba real de punta a punta del
// flujo mas usado (carga manual): click + submit real (antiforgery real, sin bypass).
[Collection("WebCore browser")]
public sealed class ElaboradosTests
{
    private readonly WebCoreFixture _fixture;

    public ElaboradosTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Carga_BuscaElaboradoAgregaIngredienteYGuardaReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Elaborados/Carga", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        // Chorizo (codigo 4, real, ya usado en el resto de esta verificacion).
        await page.FillAsync("#txtCodigoElaborado", "4");
        await page.Locator("#txtCodigoElaborado").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtElaboradoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        var nombreElaborado = await page.InputValueAsync("#txtElaboradoNombre");
        Assert.False(string.IsNullOrWhiteSpace(nombreElaborado));

        // Sal (codigo 12, ingrediente real de la formula de Chorizo).
        await page.FillAsync("#txtCodigoIngrediente", "12");
        await page.Locator("#txtCodigoIngrediente").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtIngredienteNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        await page.FillAsync("#txtKgIngrediente", "0,5");
        await page.ClickAsync("#btnAgregarIngrediente");

        await page.WaitForSelectorAsync("#tablaLineasElaborado tbody tr, table tbody tr[data-codigo]", new PageWaitForSelectorOptions { Timeout = 5000 });

        var marcador = "E2E elaborado " + DateTime.UtcNow.Ticks;
        await page.FillAsync("#Observaciones", marcador);

        await page.ClickAsync("#btnGuardarElaborado");

        // Guardado real -> SweetAlert de exito (elaborados-carga.js, showSuccessAndRedirect) y
        // redirect automatico a /Elaborados.
        await page.WaitForSelectorAsync(".swal2-popup:visible", new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.Contains("guardado correctamente", await page.Locator(".swal2-popup").InnerTextAsync());

        await page.WaitForURLAsync(url => url.Contains("/Elaborados") && !url.Contains("/Carga"), new PageWaitForURLOptions { Timeout = 10000 });

        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Item 12 de los 12 pendientes (2026-09-09, ver docs/DECISIONS.md "Batch D: scanner +
    // buscador de productos"): "en /Elaborados/EditarFormula completar todas las funcionalidades
    // faltantes (modales de buscar los productos)". El HTML/JS de los modales de busqueda y toda
    // la logica de negocio ya estaban completos con paridad 1:1 (no era un gap de logica) --
    // causa real: Scripts/app/modal-productos.js (define window.abrirBuscarProductoModal, usado
    // por los botones F9/F10) nunca se cargaba en esta vista, asi que clickear "Buscar" caia en un
    // "console.error" defensivo sin abrir nada. Fix: modal-productos.js pasa a cargarse global en
    // _Layout.cshtml (igual que scanner.js, mismo batch, mismo criterio -- ver el comentario en
    // _Layout.cshtml). Este test verifica los 2 botones de esta vista (elaborado E ingrediente).
    [Fact]
    public async Task EditarFormula_BotonesBuscar_AbrenElModalDeProductos()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Elaborados/EditarFormula/0", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.ClickAsync("#btnBuscarElaboradoFormula");
        await page.WaitForTimeoutAsync(500);
        Assert.True(await page.Locator(".modal.show").CountAsync() > 0, "El boton 'Buscar elaborado (F9)' deberia abrir el modal de busqueda de productos.");
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForTimeoutAsync(400);

        await page.ClickAsync("#btnBuscarIngredienteFormula");
        await page.WaitForTimeoutAsync(500);
        Assert.True(await page.Locator(".modal.show").CountAsync() > 0, "El boton 'Buscar ingrediente (F10)' deberia abrir el modal de busqueda de productos.");

        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
