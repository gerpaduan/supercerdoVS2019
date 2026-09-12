using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario 2026-09-11 (ver docs/DECISIONS.md "Batch 5, Fix C"): al
// editar un pago YA EXISTENTE desde la Cuenta Corriente embebida en POS, la fila hacia
// window.location.href directo (a diferencia del boton "nuevo pago", que ya usaba
// data-pos-ajax + el interceptor de POS.cshtml) -- con #modalFinanzasPOS abierto, eso disparaba
// el guard de beforeunload de pos-cart.js ("Desea salir del sitio?") y terminaba sacando al
// usuario de /Ventas/POS. Fix: CtaCtePersona.cshtml ahora llama a window.POSFinanzas.cargarPago
// en vez de navegar cuando la vista corre embebida en POS (desdePosCtaCte).
[Collection("WebCore browser")]
public sealed class EditarPagoDesdePOSTests
{
    private readonly WebCoreFixture _fixture;

    public EditarPagoDesdePOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DesdePOS_ClickEnUnPagoExistente_AbreElModalSinNavegarAfuera()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        var dialogsDisparados = new List<string>();
        page.Dialog += (_, dialog) =>
        {
            dialogsDisparados.Add(dialog.Message);
            _ = dialog.DismissAsync();
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F2");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var filaPersona = page.Locator("tr[onclick='irACtaCte(this)']").First;
        Assert.True(await filaPersona.CountAsync() > 0, "hace falta al menos una persona con cuenta corriente real en la base de dev para este test");
        await filaPersona.ClickAsync();
        await page.WaitForTimeoutAsync(600);

        // Crea un pago real para tener una fila editable sobre la cual hacer click despues.
        await page.Locator("#modalFinanzasPOS #btnAgregarPagoCtaCte").ClickAsync();
        await page.Locator("#modalPagoPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.PressAsync("2");
        await page.WaitForTimeoutAsync(300);
        await page.FillAsync("#txtImporte", "7");
        await page.ClickAsync("#modalPagoPOS #btnGuardarPago");
        await page.Locator("#modalPagoPOS.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 15000 });
        await page.WaitForTimeoutAsync(600);

        // Ahora click en una fila de pago editable real (la recien creada, la mas reciente).
        var filaPago = page.Locator(".js-fila-ctacte[data-es-pago='true']").First;
        Assert.True(await filaPago.CountAsync() > 0, "no se encontro ninguna fila de pago editable tras crear el pago");
        await filaPago.ClickAsync();
        await page.Locator("#modalPagoPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        // El bug real: sin el fix, esto navega afuera de POS y dispara el cartel de salida.
        Assert.Contains("/Ventas/POS", page.Url);
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        Assert.Empty(dialogsDisparados);

        await page.CloseAsync();
    }
}
