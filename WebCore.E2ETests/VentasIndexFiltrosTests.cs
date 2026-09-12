using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Batch 5 de la quinta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 5: alinear el
// botón Buscar con los inputs de fecha". Causa real: .filtros-fila-fechas es un grid con
// align-items:end -- las 3 celdas comparten el alto de fila (el de la mas alta) y cada una
// alinea su ULTIMO elemento al fondo de esa fila compartida. Las celdas "Desde"/"Hasta" tienen un
// <small class="fecha-resumen-dia"> como ultimo elemento; la celda del boton no tenia nada
// despues del boton, asi que quedaba alineado contra el <small> vacio de las otras celdas, no
// contra el input.
[Collection("WebCore browser")]
public sealed class VentasIndexFiltrosTests
{
    private readonly WebCoreFixture _fixture;

    public VentasIndexFiltrosTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasIndex_BotonBuscar_QuedaAlineadoConLosInputsDeFecha()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var inputBox = await page.Locator("#fechaDesde").BoundingBoxAsync();
        var btnBox = await page.Locator("#btnFiltrarFechas").BoundingBoxAsync();
        Assert.NotNull(inputBox);
        Assert.NotNull(btnBox);

        var bordeInferiorInput = inputBox!.Y + inputBox.Height;
        var bordeInferiorBoton = btnBox!.Y + btnBox.Height;
        Assert.True(System.Math.Abs(bordeInferiorInput - bordeInferiorBoton) <= 2,
            $"El botón Buscar (borde inferior {bordeInferiorBoton}) debería alinearse con el input de fecha (borde inferior {bordeInferiorInput}).");

        await page.CloseAsync();
    }
}
