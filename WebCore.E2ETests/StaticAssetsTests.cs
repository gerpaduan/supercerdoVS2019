using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion del bug real encontrado el 2026-09-05 (ver docs/DECISIONS.md): app.MapStaticAssets()
// devolvia Content-Length: 0 para jquery.min.js a cualquier cliente que pida gzip (osea, todo
// navegador real) -- rompia jQuery en TODA la app en silencio, invisible por curl (que no pide
// compresion por default). Reemplazado por app.UseStaticFiles() en Program.cs. Este test evita
// que la app vuelva a esa configuracion sin que nadie lo note.
[Collection("WebCore browser")]
public sealed class StaticAssetsTests
{
    private readonly WebCoreFixture _fixture;

    public StaticAssetsTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task JQuery_SirveElArchivoRealNoVacio()
    {
        // Playwright siempre pide gzip/br (Accept-Encoding), igual que cualquier navegador real
        // -- a diferencia de curl sin --compressed, que no dispara este bug.
        var page = await _fixture.Browser.NewPageAsync();
        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/lib/jquery/dist/jquery.min.js");

        Assert.NotNull(response);
        Assert.Equal(200, response!.Status);

        var body = await response.BodyAsync();
        Assert.True(body.Length > 50000, $"jquery.min.js vino con {body.Length} bytes -- deberia ser ~87KB.");

        await page.CloseAsync();
    }

    [Fact]
    public async Task JQuery_QuedaDefinidoEnUnaPaginaReal()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Personas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        var jqueryType = await page.EvaluateAsync<string>("typeof window.jQuery");
        Assert.Equal("function", jqueryType);

        await page.CloseAsync();
    }
}
