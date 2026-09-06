using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Puerta de entrada real (click + doble-click + submit de formulario con antiforgery real, sin
// bypass) para ProductosController.GenerarEtiquetasPdf (ver docs/10-migracion-aspnet-core/
// README.md, Modulo 3). Reemplaza la verificacion anterior via curl con
// [ValidateAntiForgeryToken] comentado temporalmente -- ahora se ejercita el flujo tal cual lo
// usaria un usuario real.
[Collection("WebCore browser")]
public sealed class GenerarEtiquetasPdfTests
{
    private readonly WebCoreFixture _fixture;

    public GenerarEtiquetasPdfTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GenerarEtiquetas_DescargaPdfRealParaProductosSeleccionados()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos?modo=etiquetas", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 20000
        });

        var filas = page.Locator("tbody tr[data-id]");
        var cantidadFilas = await filas.CountAsync();
        Assert.True(cantidadFilas >= 2, "Se necesitan al menos 2 productos reales en la base para este test.");

        // Mismo gesto que usa Index.cshtml para tildar productos en modo etiquetas (doble clic) --
        // sobre la 2da celda (nombre), no sobre la 1ra (el checkbox .chk-etiqueta): el handler de
        // la vista ignora a proposito un doble clic que caiga directo sobre el input, y un doble
        // clic nativo ahi lo tilda y destilda (neto: sin cambio) en vez de pasar por el toggle
        // manual del handler.
        await filas.Nth(0).Locator("td").Nth(1).DblClickAsync();
        await filas.Nth(1).Locator("td").Nth(1).DblClickAsync();

        var selectorTamano = page.Locator("#selTamanoEtiqueta");
        if (await selectorTamano.CountAsync() > 0)
        {
            await selectorTamano.SelectOptionAsync("mediana");
        }

        var download = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.ClickAsync("button:has-text(\"Generar etiquetas\")");
        }, new PageRunAndWaitForDownloadOptions { Timeout = 15000 });

        var rutaTemporal = Path.Combine(Path.GetTempPath(), "webcore-e2e-etiquetas.pdf");
        await download.SaveAsAsync(rutaTemporal);

        var bytes = await File.ReadAllBytesAsync(rutaTemporal);
        Assert.True(bytes.Length > 1000, "El PDF descargado es sospechosamente chico.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));

        File.Delete(rutaTemporal);
        await page.CloseAsync();
    }
}
