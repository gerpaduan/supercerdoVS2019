using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 5 del pedido del usuario, redisenado 2026-09-11 (ver docs/DECISIONS.md "Batch 6: fecha del
// POS: click-to-reveal + ventana de dias"): el chip #fechaHoraPOS ahora se muestra SIEMPRE para
// cualquier usuario -- reemplaza el gate binario anterior (PuedeOperarSinCajaYEditarFecha decidia
// si se mostraba el chip de solo lectura O el input editable). Al hacer click en el chip se revela
// #fechaVentaEditable en su lugar, validado contra una ventana de dias (DiasLimitFechaDesde,
// VentasController.FechaVentaDentroDeVentanaPermitida) en vez de por permiso. El bypass de "caja
// abierta" (el otro lado del pedido original) no se ejercita aca a proposito: cerrar la caja real
// de "ger" en la base de dev rompería el supuesto de "caja abierta" que asumen otros tests de esta
// suite -- se verificó por lectura de codigo, no en vivo.
[Collection("WebCore browser")]
public sealed class EditarFechaVentaTests
{
    private readonly WebCoreFixture _fixture;

    public EditarFechaVentaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentaNueva_ClickEnElChip_RevelaElInputEditable()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // El chip y el input coexisten en el DOM (el input arranca oculto con d-none) -- universal
        // para cualquier usuario, ya no depende de un permiso.
        var chip = page.Locator("#fechaHoraPOSChip");
        var inputFecha = page.Locator("#fechaVentaEditable");
        Assert.True(await chip.CountAsync() > 0, "no aparecio el chip de fecha (deberia ser universal)");
        Assert.True(await inputFecha.CountAsync() > 0, "no aparecio el datetime-local editable en el DOM");
        Assert.False(await inputFecha.IsVisibleAsync(), "el input editable no deberia estar visible antes del click");

        await chip.ClickAsync();
        await page.WaitForTimeoutAsync(200);

        Assert.True(await inputFecha.IsVisibleAsync(), "el click en el chip deberia revelar el input editable");
        Assert.False(await page.Locator("#fechaHoraPOSWrap").IsVisibleAsync(), "el chip deberia ocultarse al revelar el input");

        // Con caja abierta (caso real de "ger" en dev, ver WebCoreFixture.AsegurarCajaAbiertaAsync),
        // no debe mostrarse el aviso de inconsistencia.
        Assert.Equal(0, await page.Locator("#alertaCajaCerradaPOS").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task VentaNueva_CambiarFechaEditable_SincronizaElHiddenYPersisteAlFinalizar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.Locator("#fechaHoraPOSChip").ClickAsync();
        await page.WaitForTimeoutAsync(200);

        // Item 5b (2026-09-12, ver docs/DECISIONS.md): con caja real abierta (caso de "ger" en
        // esta suite, ver WebCoreFixture.AsegurarCajaAbiertaAsync), la fecha no puede ser anterior
        // a cuando se abrio esa caja -- "ayer" ya NO es valido de forma confiable (la caja se abre
        // recien al arrancar ESTA corrida, siempre el mismo dia, a veces literalmente minutos
        // antes de este test). Se lee el piso real (data-caja-apertura, expuesto en POS.cshtml
        // solo para que los tests puedan leerlo) en vez de adivinar por reloj de pared -- evita
        // una carrera con el timestamp exacto de apertura de la caja de esta corrida.
        var cajaAperturaStr = await page.Locator("#fechaVentaEditable").GetAttributeAsync("data-caja-apertura");
        Assert.False(string.IsNullOrWhiteSpace(cajaAperturaStr), "se esperaba una caja real abierta para este test (ver WebCoreFixture.AsegurarCajaAbiertaAsync)");
        var fechaBase = DateTime.Parse(cajaAperturaStr!).AddSeconds(5);
        var fechaElegida = fechaBase.ToString("yyyy-MM-ddTHH:mm");
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
        // Offset de solo 1 minuto (mismo dia que "ahora", ver comentario mas arriba) -- dia/mes/
        // año no sirven para distinguir "se persistio lo elegido" de "se revirtio a ahora", asi
        // que se busca la hora:minuto elegida en el texto (formato HH:mm, mismo criterio usado
        // por otros tests de este archivo para el chip de fecha).
        string horaMinutoElegida = fechaBase.ToString("HH:mm");
        Assert.Contains(horaMinutoElegida, detalleTexto);

        await page.CloseAsync();
    }
}
