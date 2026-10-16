using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modo "Precios por selección" de /Productos (?modo=precios, 2026-10-16, ver docs/DECISIONS.md):
// el lote de precios opera solo sobre los productos tildados en la grilla. Usa CARRE (idCorte=33)
// y CABEZA (idCorte=34) con sus precios canonicos (PreciosSeedHelper) y los restaura al terminar.
[Collection("WebCore browser")]
public sealed class PreciosPorSeleccionTests
{
    private readonly WebCoreFixture _fixture;

    public PreciosPorSeleccionTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    // Tilda una fila con el mismo gesto que el modo etiquetas (doble clic en la celda del codigo,
    // no sobre el checkbox: el handler de la vista ignora un doble clic que cae directo en el input).
    private static async Task TildarFilaAsync(IPage page, int idCorte)
    {
        await page.Locator($"tr#fila_{idCorte} td").Nth(1).DblClickAsync();
    }

    [Fact]
    public async Task AplicarPorcentajeATildados_CambiaSoloLosTildados_YLosDestilda()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        try
        {
            // Precios conocidos antes de tocar nada (fija 33 y 34 y deja la pagina en /Productos).
            await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);

            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos?modo=precios", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle
            });

            await TildarFilaAsync(page, PreciosSeedHelper.IdCorteCarre);
            await TildarFilaAsync(page, PreciosSeedHelper.IdCorteCabeza);
            Assert.Equal("2 seleccionados", (await page.Locator("#lblSeleccionados").InnerTextAsync()).Trim());

            await page.ClickAsync("#btnPorcentajeTildados");
            await page.Locator("#modalPrecio.show").WaitForAsync();
            await page.FillAsync("#PorcentajeLote", "+10");
            await page.ClickAsync("#btnGuardarPrecio");

            // Lista preliminar: exactamente los 2 tildados, con el 10% ya aplicado.
            var preview = page.Locator("#tablaVerificarPrecios tbody tr[data-id]");
            await preview.First.WaitForAsync();
            Assert.Equal(2, await preview.CountAsync());
            Assert.Equal("13.200,00", await page.Locator("#tablaVerificarPrecios tbody tr[data-id='33'] .js-precio-nuevo").InputValueAsync());
            Assert.Equal("3.080,00", await page.Locator("#tablaVerificarPrecios tbody tr[data-id='34'] .js-precio-nuevo").InputValueAsync());

            await page.ClickAsync("#btnGuardarCambiosMasivos");
            await page.Locator("#modalVerificarPrecios.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
            await page.WaitForTimeoutAsync(300);

            // La grilla muestra los precios nuevos y las filas guardadas quedaron destildadas.
            Assert.Contains("13.200,00", await page.Locator("#precio_33").InnerTextAsync());
            Assert.Contains("3.080,00", await page.Locator("#precio_34").InnerTextAsync());
            Assert.Equal("0 seleccionados", (await page.Locator("#lblSeleccionados").InnerTextAsync()).Trim());

            Assert.Empty(errors);
        }
        finally
        {
            // Restaura los precios canonicos aunque el test falle (los usan otros tests de la suite).
            await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task SinTildados_AmbosBotonesAvisan_YNoAbrenModal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos?modo=precios", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await page.ClickAsync("#btnPorcentajeTildados");
        await page.Locator(".swal2-popup").WaitForAsync();
        Assert.Contains("No seleccionaste ningún producto", await page.Locator(".swal2-popup").InnerTextAsync());
        Assert.Equal(0, await page.Locator("#modalPrecio.show").CountAsync());
        await page.ClickAsync(".swal2-confirm");

        await page.ClickAsync("#btnUnoPorUnoTildados");
        await page.Locator(".swal2-popup").WaitForAsync();
        Assert.Contains("No seleccionaste ningún producto", await page.Locator(".swal2-popup").InnerTextAsync());
        Assert.Equal(0, await page.Locator("#modalPrecio.show").CountAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
