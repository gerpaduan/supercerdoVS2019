using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Aviso de inconsistencia precio-de-lista vs precio-bonificado guardado (2026-09-07, pedido
// explicito del usuario -- ver docs/DECISIONS.md). Al reabrir una linea con bonificacion para
// editar, si el % guardado aplicado sobre el precio de lista ACTUAL no reproduce el precio
// unitario que quedo en el carrito, es porque el precio de lista cambio desde que se cargo esa
// linea -- se avisa (solo informativo, no bloquea nada).
[Collection("WebCore browser")]
public sealed class AlertaInconsistenciaPrecioTests
{
    private readonly WebCoreFixture _fixture;

    public AlertaInconsistenciaPrecioTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    // idCorte=33 es CARRE en el seed de esta base de desarrollo (confirmado via /Productos --
    // codigo comercial "1", idCorte interno 33). $12000 es el precio canonico que asumen TODOS
    // los tests de esta suite que usan CARRE (DescuentoTotalVentaTests, EdicionVentaBonificacion
    // Tests, etc.) -- el cleanup de este archivo SIEMPRE fija ese valor exacto en el finally (no
    // "restaura lo leido antes") para no arrastrar deriva de decimales entre corridas si un
    // cleanup anterior fallo silenciosamente (bug real encontrado 2026-09-07: el primer diseño
    // devolvia el precio leido dinamicamente y lo reescribia sin chequear el resultado del POST,
    // dejando el precio en un valor con decimales de otra corrida -- ver docs/DECISIONS.md).
    private const string PrecioCarreCanonico = "12000,00";

    private static async Task CambiarPrecioCarreAsync(IPage page, string precioCsv)
    {
        var ok = await page.EvaluateAsync<bool>(@"
            async (precioNuevo) => {
                const token = document.querySelector('input[name=""__RequestVerificationToken""]')?.value || '';
                const form = new URLSearchParams();
                form.set('__RequestVerificationToken', token);
                form.set('IdCorte', '33');
                form.set('PrecioKg', precioNuevo);
                const resp = await fetch('/Productos/EditPrecioCorte', { method: 'POST', body: form });
                return resp.ok;
            }", precioCsv);
        Assert.True(ok, $"No se pudo cambiar el precio de CARRE a {precioCsv} (POST a EditPrecioCorte fallo)");
    }

    [Fact]
    public async Task LineaBonificada_ConPrecioDeListaCambiado_MuestraAviso()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await CambiarPrecioCarreAsync(page, PrecioCarreCanonico);

        try
        {
            // Crear una venta con CARRE bonificado 10% (precio de lista $12000 -> $10800).
            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(500);
            await page.FillAsync("#inputCodigo", "1");
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);
            await page.FillAsync("#inputCantidad", "1");
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);

            await page.ClickAsync("#tablaItems tr.fila-item");
            await page.WaitForTimeoutAsync(500);
            await page.Keyboard.PressAsync("/");
            await page.WaitForTimeoutAsync(300);
            await page.FillAsync("#txtPrecioKg", "10800");
            await page.WaitForTimeoutAsync(300);
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);

            await page.Keyboard.PressAsync("End");
            await page.WaitForTimeoutAsync(800);
            await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
            await page.WaitForTimeoutAsync(1500);

            var resumenTexto = await page.Locator("#pvbResumenVenta").InnerTextAsync();
            var match = System.Text.RegularExpressions.Regex.Match(resumenTexto, @"#(\d+)");
            Assert.True(match.Success, "no se pudo capturar el id de la venta: " + resumenTexto);
            var idVenta = match.Groups[1].Value;

            // Cambiar el precio de lista de CARRE -> ahora hay inconsistencia con la linea ya
            // guardada (10800 ya no es "12000 con 10% off", sino "13000 con 10% off" esperado).
            await CambiarPrecioCarreAsync(page, "13000,00");

            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS?idVentaEditar={idVenta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(800);
            await page.ClickAsync("#tablaItems tr.fila-item");
            await page.WaitForTimeoutAsync(500);

            Assert.True(await page.Locator("#alertaPrecioListaInconsistente").IsVisibleAsync(),
                "la alerta de inconsistencia deberia aparecer: precio de lista cambio de 12000 a 13000 despues de bonificar la linea");
            Assert.Contains("13000", await page.Locator("#precioListaVigenteTexto").InnerTextAsync());
        }
        finally
        {
            await CambiarPrecioCarreAsync(page, PrecioCarreCanonico);
            await page.CloseAsync();
        }
    }

    [Fact]
    public async Task LineaBonificada_SinCambioDePrecio_NoMuestraAviso()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Productos", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await CambiarPrecioCarreAsync(page, PrecioCarreCanonico);

        try
        {
            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(500);
            await page.FillAsync("#inputCodigo", "1");
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);
            await page.FillAsync("#inputCantidad", "1");
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);

            await page.ClickAsync("#tablaItems tr.fila-item");
            await page.WaitForTimeoutAsync(500);
            await page.Keyboard.PressAsync("/");
            await page.WaitForTimeoutAsync(300);
            await page.FillAsync("#txtPrecioKg", "10800");
            await page.WaitForTimeoutAsync(300);
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(600);

            await page.Keyboard.PressAsync("End");
            await page.WaitForTimeoutAsync(800);
            await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
            await page.WaitForTimeoutAsync(1500);

            var resumenTexto = await page.Locator("#pvbResumenVenta").InnerTextAsync();
            var match = System.Text.RegularExpressions.Regex.Match(resumenTexto, @"#(\d+)");
            Assert.True(match.Success, "no se pudo capturar el id de la venta: " + resumenTexto);
            var idVenta = match.Groups[1].Value;

            // Sin cambiar el precio de lista: la bonificacion guardada sigue siendo consistente.
            await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS?idVentaEditar={idVenta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            await page.WaitForTimeoutAsync(800);
            await page.ClickAsync("#tablaItems tr.fila-item");
            await page.WaitForTimeoutAsync(500);

            Assert.False(await page.Locator("#alertaPrecioListaInconsistente").IsVisibleAsync(),
                "no deberia mostrarse la alerta: el precio de lista no cambio desde que se cargo la linea");
        }
        finally
        {
            await CambiarPrecioCarreAsync(page, PrecioCarreCanonico);
            await page.CloseAsync();
        }
    }
}
