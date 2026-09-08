using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario (2026-09-07, ver docs/DECISIONS.md): en /Productos, el
// detalle de CADA fila arrancaba expandido al cargar la pagina (todas las filas "solapadas" con
// su panel de detalle abierto), en vez de arrancar colapsado como corresponde. Causa: el shim
// jQuery de bootstrap4-compat.js ($(el).collapse('hide')), al construir una instancia de BS5
// Collapse por primera vez (que corre en TODAS las filas apenas carga la pagina, via
// setVistaCompleta(false) en initVista()), no pasaba toggle:false -- el constructor de BS5 (con
// su default toggle:true) auto-mostraba el panel ANTES de que corriera el 'hide' explicito, que
// quedaba abortado por el guard interno de BS5 (_isTransitioning). El plugin jQuery ORIGINAL de
// Bootstrap 4 ya tenia exactamente este guard para evitar la carrera; se replico en el shim.
[Collection("WebCore browser")]
public sealed class ProductosDetalleFilaTests
{
    private readonly WebCoreFixture _fixture;

    public ProductosDetalleFilaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Productos_NingunDetalleArrancaExpandido()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(800);

        var total = await page.EvaluateAsync<int>("document.querySelectorAll('.tr-detalles .collapse').length");
        Assert.True(total > 0, "no se encontro ninguna fila de detalles -- ¿cambio el fixture de datos?");

        var expandidas = await page.EvaluateAsync<int>("document.querySelectorAll('.tr-detalles .collapse.show').length");
        Assert.Equal(0, expandidas);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Productos_ToggleIndividualYVistaCompletaFuncionan()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(800);

        var boton = page.Locator(".btn-detalles").First;
        var collapse1 = page.Locator(".tr-detalles .collapse").First;

        await boton.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        Assert.True(await collapse1.EvaluateAsync<bool>("el => el.classList.contains('show')"));

        await boton.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        Assert.False(await collapse1.EvaluateAsync<bool>("el => el.classList.contains('show')"));

        await page.Locator("#switchVistaCompleta").ClickAsync();
        await page.WaitForTimeoutAsync(800);
        var total = await page.EvaluateAsync<int>("document.querySelectorAll('.tr-detalles .collapse').length");
        var expandidas = await page.EvaluateAsync<int>("document.querySelectorAll('.tr-detalles .collapse.show').length");
        Assert.Equal(total, expandidas);

        await page.Locator("#switchVistaCompleta").ClickAsync();
        await page.WaitForTimeoutAsync(800);
        var expandidasTrasOff = await page.EvaluateAsync<int>("document.querySelectorAll('.tr-detalles .collapse.show').length");
        Assert.Equal(0, expandidasTrasOff);

        await page.CloseAsync();
    }
}
