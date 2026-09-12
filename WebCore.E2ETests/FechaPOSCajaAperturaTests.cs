using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 5b (2026-09-12, ver docs/DECISIONS.md "rediseño de la validación de fecha del POS"):
// primera validación -- con caja real abierta, la fecha no puede ser anterior a cuando se abrió
// (antes de este batch, solo regía la ventana de días global). Se prueba con una fecha muy vieja
// (2020), imposible de estar dentro de la apertura de CUALQUIER caja real -- verifica el mecanismo
// de rechazo sin depender del timestamp exacto de apertura de la caja de "ger" en la base de dev.
[Collection("WebCore browser")]
public sealed class FechaPOSCajaAperturaTests
{
    private readonly WebCoreFixture _fixture;

    public FechaPOSCajaAperturaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task FechaAnteriorAAperturaDeCaja_SeRevierteAAhora()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.Locator("#fechaHoraPOSChip").ClickAsync();
        await page.WaitForTimeoutAsync(200);

        // Sin label "Fecha y hora" (item 5a) -- confirma que se saco.
        Assert.Equal(0, await page.Locator("label[for='fechaVentaEditable']").CountAsync());

        var antesDeAbrirCualquierCaja = "2020-01-01T00:00";
        await page.Locator("#fechaVentaEditable").FillAsync(antesDeAbrirCualquierCaja);
        await page.Locator("#fechaVentaEditable").DispatchEventAsync("change");
        await page.WaitForTimeoutAsync(200);

        var valorFinal = await page.Locator("#fechaVentaEditable").InputValueAsync();
        var fechaFinal = DateTime.Parse(valorFinal);
        Assert.True(fechaFinal.Year >= DateTime.Now.Year, $"la fecha de 2020 deberia haberse revertido a ahora, quedo en '{valorFinal}'");

        var hiddenFecha = await page.Locator("#fechaVenta").InputValueAsync();
        Assert.DoesNotContain("2020", hiddenFecha);

        await page.CloseAsync();
    }
}
