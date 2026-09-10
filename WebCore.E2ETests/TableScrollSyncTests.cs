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
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 480, Height = 800 } // fuerza overflow horizontal
        });
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        // idSucursal=2 (San Lorenzo) explicito: desde el login real (2026-09-06) el default ya no
        // es el stub hardcodeado sino la sucursal real de "ger" (San Martin=1), que no tiene datos
        // de stock en el rango de fecha por defecto -- la tabla quedaria vacia sin esto.
        //
        // fechaDesde tambien explicito (2026-09-09, ver docs/DECISIONS.md "Fix: 2 tests de Stock
        // con fecha por defecto"): StockController.Lineas() usa desde=DateTime.Today cuando no se
        // pasa fechaDesde -- la tabla queda vacia salvo que exista un movimiento cargado ESE MISMO
        // dia. La base compartida de desarrollo no se re-siembra a diario, asi que confiar en el
        // default hacia que este test fallara solo por el paso del calendario real, sin relacion
        // con ningun cambio de codigo. 1 año atras cubre cualquier dato de seed existente sin
        // necesidad de mantener una fecha fija a mano.
        string fechaDesde = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Lineas?idSucursal=2&fechaDesde={fechaDesde}", new PageGotoOptions
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
