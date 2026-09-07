using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Historial de precios de cliente en POS (F8 / boton #btnHistorialPreciosCliente), 2026-09-07.
// Bug real reportado por el usuario: el boton visible nunca tenia su propio handler de click --
// solo el atajo F8 abria el modal (ver docs/DECISIONS.md). Cubre ademas el filtro de lineas
// anuladas (cantkg > 0) agregado el mismo dia en obtenerUltimosPreciosPorCliente.
[Collection("WebCore browser")]
public sealed class HistorialPreciosClienteTests
{
    private readonly WebCoreFixture _fixture;

    public HistorialPreciosClienteTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ClickEnBoton_AbreModalConHistorialDelClienteSeleccionado()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // Consumidor Final: el boton arranca oculto.
        Assert.False(await page.Locator("#btnHistorialPreciosCliente").IsVisibleAsync());

        // Seleccionar un cliente real (no Consumidor Final) via el buscador F9 -- requiere doble
        // clic o Enter, un clic simple solo resalta la fila (mismo comportamiento que MVC clasico).
        await page.Keyboard.PressAsync("F9");
        await page.WaitForTimeoutAsync(500);
        await page.FillAsync("#filtroPersona", "JUAN PEREZ");
        await page.WaitForTimeoutAsync(800);
        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(800);

        Assert.True(await page.Locator("#btnHistorialPreciosCliente").IsVisibleAsync());

        await page.ClickAsync("#btnHistorialPreciosCliente");
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.Locator("#contenedorFinanzasPOS .historial-precios-tabla").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        // Todas las filas mostradas deben tener precio > 0 (lineas anuladas/cantidad negativa
        // quedan afuera, ver docs/DECISIONS.md 2026-09-07).
        var precios = await page.Locator("#contenedorFinanzasPOS .historial-precios-tabla tbody tr td:nth-child(3)").AllInnerTextsAsync();
        Assert.NotEmpty(precios);
        foreach (var precioTexto in precios)
        {
            var monto = decimal.Parse(precioTexto.Replace("$", "").Trim(), System.Globalization.CultureInfo.GetCultureInfo("es-AR"));
            Assert.True(monto > 0, $"precio inesperado <= 0: {precioTexto}");
        }

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
