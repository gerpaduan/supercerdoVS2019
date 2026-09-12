using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 8 de la quinta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 8: advertencia
// de salir sin guardar despues de guardar una compra". Causa raiz: EditPageGuard fija
// allowExit=true en el submit y arranca un timer de 1500ms que lo revierte solo si nadie lo
// cancela -- compras.js mostraba un Swal de exito con timer:1800ms antes de navegar, 300ms MAS
// que el del guard, asi que para cuando navegaba la proteccion ya se habia reactivado.
[Collection("WebCore browser")]
public sealed class ComprasAdvertenciaSalirTests
{
    private readonly WebCoreFixture _fixture;

    public ComprasAdvertenciaSalirTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GuardarCompraCorrectamente_NoDejaLaAdvertenciaDeSalirColgada()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var dialogs = new List<string>();
        page.Dialog += async (_, dialog) =>
        {
            dialogs.Add(dialog.Message);
            await dialog.DismissAsync();
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras/NuevaCompra", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F9");
        await page.Locator("#modalBuscarPersona.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.FillAsync("#filtroPersona", "a");
        await page.WaitForTimeoutAsync(600);
        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(800);
        await page.FillAsync("#txtCantKgs", "1");
        await page.FillAsync("#txtPrecioKg", "100");
        await page.ClickAsync("#btnAgregarLineaCorte");
        await page.WaitForTimeoutAsync(500);
        Assert.Equal(1, await page.Locator("#tablaLineasCompra tbody tr").CountAsync());

        dialogs.Clear();
        await page.ClickAsync("#btnGuardarCompra");
        await page.WaitForTimeoutAsync(3000);

        // Guardado real: redirige a Compras/Index. Sin ningun dialog nativo durante el guardado
        // (ni de validacion, ni de "salir sin guardar").
        Assert.Contains("/Compras", page.Url);
        Assert.Empty(dialogs);

        // La prueba real del bug: navegar afuera DESPUES de guardar no debe disparar la
        // advertencia nativa de "salir sin guardar" (antes del fix, esto fallaba de forma
        // intermitente segun el timing exacto del Swal de 1800ms vs el timer de 1500ms del guard).
        dialogs.Clear();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 10000 });
        Assert.Empty(dialogs);

        await page.CloseAsync();
    }
}
