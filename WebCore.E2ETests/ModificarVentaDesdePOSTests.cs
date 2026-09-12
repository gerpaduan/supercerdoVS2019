using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario (2026-09-11, ver docs/DECISIONS.md "Batch 1: returnUrl al
// modificar venta desde POS"): al modificar una venta desde POS, al guardar se redirigia siempre
// al layout de DetalleVenta en vez de volver a POS -- forma-pago.js:699-720 ya sabia leer
// window.POSModo.returnUrl al terminar de guardar, pero VentasController.POS() nunca recibia ni
// pasaba ese dato a la vista. Mismo patron de test que EdicionVentaBonificacionTests.cs (navegar
// directo a POS?idVentaEditar=X, sin pasar por el modal de Actividades) -- se navega tambien con
// returnUrl=/Ventas/POS en la URL, exactamente el valor que _DetalleVentaCard.cshtml ahora arma
// cuando el link "Modificar venta" se clickea desde dentro de POS (desdePos=true).
[Collection("WebCore browser")]
public sealed class ModificarVentaDesdePOSTests
{
    private readonly WebCoreFixture _fixture;

    public ModificarVentaDesdePOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ModificarVenta_ConReturnUrlDePOS_AlGuardarVuelveAPOS()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // Crear una venta real para tener algo que modificar.
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
        Assert.True(match.Success, "no se pudo capturar el id de la venta recien creada: " + resumenTexto);
        var idVenta = match.Groups[1].Value;

        // Reabrir en modo "modificar" con returnUrl=/Ventas/POS -- mismo valor que
        // _DetalleVentaCard.cshtml arma cuando desdePos es true.
        await page.GotoAsync(
            $"{WebCoreFixture.BaseUrl}/Ventas/POS?idVentaEditar={idVenta}&returnUrl=%2FVentas%2FPOS",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(800);

        var returnUrlEnJs = await page.EvaluateAsync<string>("window.POSModo?.returnUrl || ''");
        Assert.Equal("/Ventas/POS", returnUrlEnJs);

        // Agregar otro producto y guardar la modificacion.
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");

        // El bug real: sin el fix, esto termina en /Ventas/DetalleVenta?id=X. Con el fix, vuelve a
        // /Ventas/POS (una instancia nueva, sin idVentaEditar).
        await page.WaitForURLAsync(u => u.ToString().Contains("/Ventas/POS"), new PageWaitForURLOptions { Timeout = 15000 });
        Assert.Contains("/Ventas/POS", page.Url);
        Assert.DoesNotContain("/Ventas/DetalleVenta", page.Url);

        await page.CloseAsync();
    }
}
