using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Atajo "Enter = mantener la misma forma de pago" al modificar una venta (2026-09-07, pedido
// explicito del usuario -- ver docs/DECISIONS.md). Solo aplica en edicion de venta real
// (esEdicionVenta), donde "Forma de pago actual" es un dato real de la venta -- muestra un hint
// visible en el modal y, al presionar Enter (fuera de cualquier input), dispara la misma forma de
// pago ya usada sin tener que re-seleccionarla con el mouse o el atajo numerico.
[Collection("WebCore browser")]
public sealed class MantenerFormaPagoTests
{
    private readonly WebCoreFixture _fixture;

    public MantenerFormaPagoTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ModificarVenta_MuestraHintYEnterMantieneLaFormaDePagoActual()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // Crear una venta con Efectivo.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1500);

        var resumenTexto = await page.Locator("#pvbResumenVenta").InnerTextAsync();
        var match = System.Text.RegularExpressions.Regex.Match(resumenTexto, @"#(\d+)");
        Assert.True(match.Success, "no se pudo capturar el id de la venta: " + resumenTexto);
        var idVenta = match.Groups[1].Value;

        // Reabrir para modificar y confirmar el hint + el atajo.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS?idVentaEditar={idVenta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(800);
        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);

        Assert.Equal(1, await page.Locator("#modalFormaPago.show").CountAsync());
        var textoInfo = await page.Locator("#formaPagoActualInfo").InnerTextAsync();
        Assert.Contains("Forma de pago actual: Efectivo", textoInfo);
        Assert.Contains("Enter", textoInfo);
        Assert.Contains("mantener", textoInfo, StringComparison.OrdinalIgnoreCase);

        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(1000);

        // El modal se cierra solo si finalizarVenta/ModificarVenta se disparo con exito.
        Assert.Equal(0, await page.Locator("#modalFormaPago.show").CountAsync());

        await page.CloseAsync();
    }
}
