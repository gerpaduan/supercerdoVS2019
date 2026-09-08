using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Mascara de miles/coma en vivo + Enter guarda, en el modal "Modificar precio" de /Productos
// (2026-09-07, pedido explicito del usuario -- ver docs/DECISIONS.md). Port de
// Web/Scripts/app/money-input-mask.js (ya usado en Finanzas/Pagos del clasico, nunca aplicado al
// precio de producto ni en el clasico ni en WebCore hasta ahora).
[Collection("WebCore browser")]
public sealed class PrecioProductoMaskTests
{
    // $12000 es el precio canonico de CARRE (idCorte=33) que asumen TODOS los tests de esta suite
    // que tocan ese producto (DescuentoTotalVentaTests, EdicionVentaBonificacionTests,
    // AlertaInconsistenciaPrecioTests). Fijar SIEMPRE este valor exacto al inicio y al final (no
    // "restaurar lo leido antes") evita el bug real encontrado 2026-09-07 (ver docs/DECISIONS.md):
    // si un test anterior dejaba el precio corrupto por un cleanup fallido en silencio, restaurar
    // "el valor leido" propagaba el error en vez de corregirlo.
    private const string PrecioCarreCanonico = "12000,00";

    private readonly WebCoreFixture _fixture;

    public PrecioProductoMaskTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<bool> FijarPrecioCarreAsync(IPage page)
    {
        return await page.EvaluateAsync<bool>(@"
            async (precio) => {
                const token = document.querySelector('input[name=""__RequestVerificationToken""]')?.value || '';
                const form = new URLSearchParams();
                form.set('__RequestVerificationToken', token);
                form.set('IdCorte', '33');
                form.set('PrecioKg', precio);
                const resp = await fetch('/Productos/EditPrecioCorte', { method: 'POST', body: form });
                return resp.ok;
            }", PrecioCarreCanonico);
    }

    [Fact]
    public async Task ModalPrecio_MuestraSeparadorDeMilesEnVivo_YActualizaVariacionPrecio()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.ClickAsync("button[title='Modificar precio']");
        await page.WaitForTimeoutAsync(1000);

        await page.FocusAsync("#PrecioKg");
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.TypeAsync("1234567");
        await page.WaitForTimeoutAsync(200);

        Assert.Equal("1.234.567", await page.Locator("#PrecioKg").InputValueAsync());
        // El % de variacion debe recalcularse en vivo (no solo al perder el foco).
        Assert.Contains("%", await page.Locator("#precioVariacion").InnerTextAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ModalPrecio_Enter_GuardaYCierraElModal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // Fija el precio a un valor canonico conocido ANTES de tocar nada -- usa siempre CARRE
        // (idCorte=33) en vez de "el primer boton de la lista" (fragil ante reordenamientos), asi
        // el cleanup nunca depende de "lo que hubiera antes" (ver comentario de PrecioCarreCanonico).
        Assert.True(await FijarPrecioCarreAsync(page), "no se pudo fijar el precio canonico antes de la prueba");
        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(300);

        try
        {
            await page.ClickAsync("button[onclick^=\"abrirPopupPrecio(33,\"]");
            await page.WaitForTimeoutAsync(1000);

            await page.FocusAsync("#PrecioKg");
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.TypeAsync("12345");
            await page.WaitForTimeoutAsync(200);
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(1000);

            Assert.Equal(0, await page.Locator("#modalPrecio.show").CountAsync());
            Assert.Empty(errors); // cubre el ReferenceError de manejarPermiso ya corregido
        }
        finally
        {
            Assert.True(await FijarPrecioCarreAsync(page), "no se pudo restaurar el precio canonico de CARRE al finalizar");
            await page.CloseAsync();
        }
    }
}
