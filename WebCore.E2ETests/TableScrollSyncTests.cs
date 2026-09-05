using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del gap cerrado 2026-09-05 (ver docs/DECISIONS.md): el HTML de las vistas con tablas
// anchas ya tenia la clase .js-sync-scroll-body desde que se portaron, pero _Layout.cshtml nunca
// cargaba table-scroll-sync.js ni su CSS -- la barra de scroll flotante nunca aparecia. Se agrego
// el script + CSS globalmente (sin tocar ninguna vista, el markup ya estaba listo).
[Collection("WebCore browser")]
public sealed class TableScrollSyncTests
{
    private readonly WebCoreFixture _fixture;

    public TableScrollSyncTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task StockLineas_GeneraBarraFlotanteEnTablaAncha()
    {
        var page = await _fixture.Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 480, Height = 800 } // fuerza overflow horizontal
        });
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Lineas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });
        await page.WaitForTimeoutAsync(500);

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);
        Assert.True(await page.Locator(".sync-scroll-host").CountAsync() > 0, "table-scroll-sync.js no envolvio ninguna .js-sync-scroll-body -- ¿se saco el script del layout?");
        Assert.True(await page.Locator(".sync-scroll-floating:visible").CountAsync() > 0, "La barra flotante no quedo visible pese a que la tabla desborda -- ¿se saco el CSS de ui-refresh.css?");

        await page.CloseAsync();
    }
}
