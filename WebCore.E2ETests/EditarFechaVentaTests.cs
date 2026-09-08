using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 5 del pedido del usuario (2026-09-07, ver docs/DECISIONS.md "Editar fecha de venta en curso
// + saltear caja abierta"): un usuario con permiso real de "modificar venta" puede editar la
// fecha de la venta EN CURSO (todavia sin finalizar) desde un datetime-local en el POS, en vez del
// chip de solo lectura #fechaHoraPOS. El usuario de dev ("ger") es Admin, y
// PuedeOperarSinCajaYEditarFecha(user) devuelve true directo para admins -- cubre el caso real
// mas comun (el admin siempre puede). El bypass de "caja abierta" (el otro lado del mismo pedido)
// no se ejercita aca a proposito: cerrar la caja real de "ger" en la base de dev rompería el
// supuesto de "caja abierta" que asumen otros tests de esta suite -- se verificó por lectura de
// codigo (mismo patron ya probado que PuedeModificarUltimaVenta), no en vivo.
[Collection("WebCore browser")]
public sealed class EditarFechaVentaTests
{
    private readonly WebCoreFixture _fixture;

    public EditarFechaVentaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentaNueva_ConPermiso_MuestraFechaEditableEnVezDelChipReadonly()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var inputFecha = page.Locator("#fechaVentaEditable");
        Assert.True(await inputFecha.CountAsync() > 0, "no aparecio el datetime-local editable para un usuario admin");
        Assert.Equal(0, await page.Locator("#fechaHoraPOS").CountAsync());

        // Con caja abierta (caso real de "ger" en dev), no debe mostrarse el aviso de inconsistencia.
        Assert.Equal(0, await page.Locator("#alertaCajaCerradaPOS").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task VentaNueva_CambiarFechaEditable_SincronizaElHiddenYPersisteAlFinalizar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var fechaElegida = "2026-01-15T09:30";
        await page.Locator("#fechaVentaEditable").FillAsync(fechaElegida);
        await page.Locator("#fechaVentaEditable").DispatchEventAsync("change");
        await page.WaitForTimeoutAsync(200);

        Assert.StartsWith(fechaElegida, await page.Locator("#fechaVenta").InputValueAsync());

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1500);

        var resumenTexto = await page.Locator("#pvbResumenVenta").InnerTextAsync();
        var match = System.Text.RegularExpressions.Regex.Match(resumenTexto, @"#(\d+)");
        Assert.True(match.Success, "no se pudo capturar el id de la venta recien creada: " + resumenTexto);
        var idVenta = match.Groups[1].Value;

        var detalleTexto = await page.EvaluateAsync<string>($@"
            async () => {{
                const resp = await fetch('/Ventas/DetalleVenta?id={idVenta}&modal=true');
                return await resp.text();
            }}");
        var matchFecha = System.Text.RegularExpressions.Regex.Match(detalleTexto, @"\b(202\d[-/]\d{1,2}[-/]\d{1,2}|\d{1,2}[-/]\d{1,2}[-/]202\d)[^<\n]*");
        Assert.True(matchFecha.Success, "no se encontro ninguna fecha en el detalle de la venta. Longitud del texto: " + detalleTexto.Length);
        Assert.True(
            matchFecha.Value.Contains("2026") && (matchFecha.Value.Contains("15") ) && (matchFecha.Value.Contains("01") || matchFecha.Value.Contains("1/") || matchFecha.Value.Contains("/1/")),
            "la fecha encontrada no coincide con la elegida (2026-01-15): '" + matchFecha.Value + "'");

        await page.CloseAsync();
    }
}
