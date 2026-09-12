using Microsoft.Playwright;

namespace WebCore.E2ETests;

// 2 bugs reales reportados por el usuario 2026-09-12 (ver docs/DECISIONS.md "Batch 4"):
// (a) Escape no cerraba #modalFinanzasPOS mostrando Cuenta Corriente de una persona (data-keyboard
//     ="false" bloqueaba TODO uso de Escape, sin importar el contenido).
// (b) el filtro de fecha/"Mostrar anulados" de CtaCtePersona.cshtml era un <form method="get">
//     plano sin desdePos -- disparaba el cartel de salida y sacaba de POS en vez de filtrar
//     adentro.
[Collection("WebCore browser")]
public sealed class CtaCtePersonaEscapeYFiltroTests
{
    private readonly WebCoreFixture _fixture;

    public CtaCtePersonaEscapeYFiltroTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirCtaCtePersonaEnPOSAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("F2");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(500);

        var fila = page.Locator("tr[onclick='irACtaCte(this)']").First;
        Assert.True(await fila.CountAsync() > 0, "hace falta al menos una persona con cuenta corriente real en la base de dev para este test");
        await fila.ClickAsync();
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#frmFiltroCtaCte").CountAsync());

        return page;
    }

    [Fact]
    public async Task Escape_CierraModalFinanzasPOS_MostrandoCtaCtePersona()
    {
        var page = await AbrirCtaCtePersonaEnPOSAsync(_fixture);

        await page.Keyboard.PressAsync("Escape");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });

        // Sigue en POS -- Escape cerro el modal, no navego afuera.
        Assert.Contains("/Ventas/POS", page.Url);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtrar_NoDisparaCartelDeSalidaYFiltraDentroDePOS()
    {
        var page = await AbrirCtaCtePersonaEnPOSAsync(_fixture);

        var dialogsDisparados = new List<string>();
        page.Dialog += (_, dialog) =>
        {
            dialogsDisparados.Add(dialog.Message);
            _ = dialog.DismissAsync();
        };

        var fechaAyer = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
        await page.FillAsync("#ctacteFechaDesde", fechaAyer);
        await page.ClickAsync("#btnFiltrarCtaCte");
        await page.WaitForTimeoutAsync(800);

        // El bug real: sin el fix, esto navegaba de pagina completa y sacaba de POS.
        Assert.Contains("/Ventas/POS", page.Url);
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        Assert.Equal(1, await page.Locator("#frmFiltroCtaCte").CountAsync());
        Assert.Empty(dialogsDisparados);

        await page.CloseAsync();
    }
}
