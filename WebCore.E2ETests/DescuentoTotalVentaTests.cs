using Microsoft.Playwright;
using System.Text.Json;

namespace WebCore.E2ETests;

// Bonificacion por % a todos los productos desde el modal de Forma de Pago (2026-09-06, pedido
// explicito del usuario; UI rediseñada 2026-09-07 -- ver docs/DECISIONS.md). El atajo "/" despliega
// un bloque con el % (negativo = recargo) -- solo disponible si NINGUN producto del carrito ya
// tiene su propia bonificacion. "Total de la Venta" (azul, #totalVenta) pasa a mostrar SIEMPRE el
// monto real a cobrar (con el % aplicado si esta activo); el bloque verde
// (#lblTotalConDescuentoTotalVenta) muestra el MONTO del descuento/recargo en si, no el total
// resultante. Al elegir la forma de pago, el % se aplica a TODAS las lineas del carrito (misma
// funcion que usa el modal "Linea de venta" con "Aplicar a todos") ANTES de mandar la venta al
// servidor.
[Collection("WebCore browser")]
public sealed class DescuentoTotalVentaTests
{
    private readonly WebCoreFixture _fixture;

    public DescuentoTotalVentaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DescuentoTotalVenta_SeAplicaATodasLasLineasAntesDeGuardar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        string? payloadCapturado = null;

        // Intercepta el POST real a FinalizarVenta para inspeccionar el payload exacto SIN crear
        // una venta real en la base compartida de desarrollo (responde con un ok:false canned).
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            payloadCapturado = route.Request.PostData;
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = "{\"ok\":false,\"msg\":\"interceptado por test, no se guardo nada\"}"
            });
        });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // 2 productos distintos, ninguno bonificado individualmente. Codigo 1 = CARRE ($12000/kg),
        // codigo 2 = CABEZA ($2800/kg) -- mismos productos de seed que usan otros tests de POS.
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "2");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.FillAsync("#inputCodigo", "2");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "3");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        Assert.Equal(1, await page.Locator("#modalFormaPago.show").CountAsync());

        var totalOriginal = await page.Locator("#totalVenta").InputValueAsync();
        Assert.Equal("32.400,00", totalOriginal);
        Assert.True(await page.Locator("#btnTogglePorcentajeTotalVenta").IsVisibleAsync());

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(400);
        Assert.True(await page.Locator("#bloquePorcentajeTotalVenta").IsVisibleAsync());

        await page.FillAsync("#txtPorcentajeTotalVenta", "10");
        await page.WaitForTimeoutAsync(400);
        // Verde = monto del descuento (10% de 32.400 = 3.240). Azul = total CON descuento (29.160).
        Assert.Contains("3.240,00", await page.Locator("#lblTotalConDescuentoTotalVenta").InnerTextAsync());
        Assert.Equal("Descuento aplicado", await page.Locator("#lblEtiquetaMontoDescuento").InnerTextAsync());
        Assert.Equal("29.160,00", await page.Locator("#totalVenta").InputValueAsync());
        Assert.False(await page.Locator("#lblTotalVentaConDescuentoTag").IsHiddenAsync());

        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1000);

        Assert.NotNull(payloadCapturado);
        var lineas = JsonDocument.Parse(payloadCapturado!).RootElement.GetProperty("lineasVenta");
        var vistas = lineas.EnumerateArray().ToList();
        Assert.Equal(2, vistas.Count);
        foreach (var linea in vistas)
        {
            Assert.Equal(10, linea.GetProperty("Bonificacion").GetDouble());
        }
        // Codigo 1 (CARRE, $12000) -10% = $10800; codigo 2 (CABEZA, $2800) -10% = $2520.
        Assert.Equal(10800, vistas.First(l => l.GetProperty("Codigo").GetInt32() == 1).GetProperty("PrecioKg").GetDouble());
        Assert.Equal(2520, vistas.First(l => l.GetProperty("Codigo").GetInt32() == 2).GetProperty("PrecioKg").GetDouble());

        Assert.Empty(errors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task DescuentoTotalVenta_BloqueadoSiYaHayLineaBonificadaIndividualmente()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        // Bonificar la linea individualmente desde el modal "Linea de venta".
        await page.ClickAsync("#tablaItems tr.fila-item");
        await page.WaitForTimeoutAsync(500);
        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        await page.FillAsync("#txtPrecioKg", "10000");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.Locator(".swal2-popup").IsVisibleAsync());
        Assert.Contains("ya existen productos bonificados", await page.Locator(".swal2-html-container").InnerTextAsync());
        Assert.False(await page.Locator("#bloquePorcentajeTotalVenta").IsVisibleAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task DescuentoTotalVenta_AlQuitarloElAzulVuelveAlTotalOriginalSinTag()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await PreciosSeedHelper.FijarPreciosCanonicosAsync(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);

        var totalOriginal = await page.Locator("#totalVenta").InputValueAsync();
        Assert.True(await page.Locator("#lblTotalVentaConDescuentoTag").IsHiddenAsync());

        // Abrir el bloque (foco queda en el input) y tipear un %.
        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        await page.Keyboard.TypeAsync("20");
        await page.WaitForTimeoutAsync(300);
        Assert.NotEqual(totalOriginal, await page.Locator("#totalVenta").InputValueAsync());
        Assert.False(await page.Locator("#lblTotalVentaConDescuentoTag").IsHiddenAsync());

        // "/" con el input editable y foco puesto no hace nada (guarda ya existente): hay que
        // sacar el foco primero para poder volver a alternar el bloque.
        await page.EvaluateAsync("document.activeElement && document.activeElement.blur()");
        await page.WaitForTimeoutAsync(200);
        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);

        Assert.False(await page.Locator("#bloquePorcentajeTotalVenta").IsVisibleAsync());
        Assert.Equal(totalOriginal, await page.Locator("#totalVenta").InputValueAsync());
        Assert.True(await page.Locator("#lblTotalVentaConDescuentoTag").IsHiddenAsync());

        await page.CloseAsync();
    }
}
