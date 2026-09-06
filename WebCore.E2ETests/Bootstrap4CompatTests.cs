using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del bug real encontrado el 2026-09-04 y cerrado el 2026-09-05 (ver docs/DECISIONS.md
// y docs/10-migracion-aspnet-core/gaps.md): WebCore corre Bootstrap 5.3.3, que elimino el plugin
// jQuery ($.fn.modal/.collapse/.alert/etc.). El markup y JS portados desde Web (16+ archivos:
// AddOrEditPago, Stock/Index, Compras/Index, Personas/Index, Productos/Index, calculadora-billetes.js,
// stock.js, egresos-caja.js, forma-pago.js, compras.js, y varios mas de POS) siguen llamando a esos
// metodos como jQuery plugin -- sin el shim, tira "$(...).modal is not a function" (pageerror real)
// y puede cortar el resto del bloque <script>. Resuelto extendiendo bootstrap4-compat.js con un
// puente jQuery -> API real de BS5 (getOrCreateInstance), en vez de reescribir cada llamada.
[Collection("WebCore browser")]
public sealed class Bootstrap4CompatTests
{
    private readonly WebCoreFixture _fixture;

    public Bootstrap4CompatTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task JQueryFnModalCollapseAlert_QuedanDefinidosComoPluginReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        Assert.Empty(errors);
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.modal === 'function'"));
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.collapse === 'function'"));
        Assert.True(await page.EvaluateAsync<bool>("typeof window.jQuery.fn.alert === 'function'"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task StockEditar_SinErrorDeScriptAlCargar()
    {
        // Repro original del gap (docs/10-migracion-aspnet-core/gaps.md, ya cerrado): esta vista
        // tiraba "$(...).modal is not a function" apenas cargaba la pagina, antes de cualquier click.
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Editar?idCorte=20", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
