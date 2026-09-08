using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario (2026-09-07, ver docs/DECISIONS.md): al modificar una venta
// ya guardada, si una linea ya tenia bonificacion, el modal "Linea de venta" recalculaba el %
// contra el precio YA BONIFICADO (guardado en LineaVenta.PrecioKg) en vez del precio de lista --
// aplicando el descuento en cascada si el usuario tocaba el % de nuevo. Fix: Ventas/POS.cshtml
// arma "precioOriginal" desde linea.Corte.PrecioKg (el precio de lista ACTUAL, via el JOIN en
// vivo que ya hace VentaPg.getVentaById), no desde linea.PrecioKg.
[Collection("WebCore browser")]
public sealed class EdicionVentaBonificacionTests
{
    private readonly WebCoreFixture _fixture;

    public EdicionVentaBonificacionTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ModificarVenta_LineaConBonificacion_RecuperaElPrecioDeListaComoBase()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);

        // Crear una venta real con CARRE (codigo 1, $12000/kg) bonificado 10% -> $10800.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.ClickAsync("#tablaItems tr.fila-item");
        await page.WaitForTimeoutAsync(500);
        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        await page.FillAsync("#txtPrecioKg", "10800");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1500);

        var resumenTexto = await page.Locator("#pvbResumenVenta").InnerTextAsync();
        var match = System.Text.RegularExpressions.Regex.Match(resumenTexto, @"#(\d+)");
        Assert.True(match.Success, "no se pudo capturar el id de la venta recien creada: " + resumenTexto);
        var idVenta = match.Groups[1].Value;

        // Reabrir la venta en modo "modificar" y verificar el estado del modal de linea.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS?idVentaEditar={idVenta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("#tablaItems tr.fila-item");
        await page.WaitForTimeoutAsync(500);

        Assert.Equal("10800.00", await page.Locator("#txtPrecioKg").InputValueAsync());
        Assert.Equal("10", await page.Locator("#txtPorcentaje").InputValueAsync());
        // La base del calculo (precio de lista) debe ser 12000, NO 10800 (el bug reportado: antes
        // del fix, precioOriginal se armaba desde linea.PrecioKg -- el precio YA bonificado -- en
        // vez de linea.Corte.PrecioKg, el precio de lista actual del producto).
        Assert.Equal("12000.00", await page.Locator("#modalPrecio").InputValueAsync());
        Assert.Equal("12000", await page.EvaluateAsync<string>("$('#modalPrecio').data('precio')?.toString()"));

        await page.CloseAsync();
    }
}
