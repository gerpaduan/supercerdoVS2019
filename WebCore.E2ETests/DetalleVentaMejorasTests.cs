using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Quinta ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md "Batches 1-4: DetalleVenta". Varios
// de estos comportamientos ya existian identicos en el clasico (bonificacion no visible,
// alineacion de decimales, negrita mezclada con text-muted) -- son mejoras nuevas pedidas ahora
// sobre una vista que WebCore ya portaba fielmente, no fixes de paridad.
[Collection("WebCore browser")]
public sealed class DetalleVentaMejorasTests
{
    private readonly WebCoreFixture _fixture;

    public DetalleVentaMejorasTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    // Batch 1: bonificacion en las lineas, mismo texto/formato que el carrito del POS
    // (pos-cart.js). Crea una venta real con una linea bonificada individualmente (mismo flujo
    // que DescuentoTotalVentaTests) para confirmar que Bonificacion sobrevive el round-trip a la
    // base (leido de vuelta via _oVentaN.getVentaById) -- no solo que el campo exista en la
    // entidad.
    [Fact]
    public async Task DetalleVenta_LineaBonificada_MuestraElPorcentaje()
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

        // Bonificar la linea individualmente (mismo patron que DescuentoTotalVentaTests).
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
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1200);

        var ventaId = await page.EvaluateAsync<string>("() => String($('#btnPvbImprimir').data('venta-id') || '')");
        Assert.False(string.IsNullOrWhiteSpace(ventaId));

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id={ventaId}", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var textoLinea = await page.Locator(".venta-lineas").InnerTextAsync();
        Assert.Contains("Bonif:", textoLinea);

        await page.CloseAsync();
    }

    // Regresion del batch 1: una venta SIN bonificacion no debe mostrar ningun texto de
    // "Bonif"/"Recargo" -- usa la venta real de seed (id 1755) ya usada por FacturaElectronicaTests.
    [Fact]
    public async Task DetalleVenta_LineaSinBonificacion_NoMuestraTextoExtra()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=1755", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var textoLinea = await page.Locator(".venta-lineas").InnerTextAsync();
        Assert.DoesNotContain("Bonif:", textoLinea);
        Assert.DoesNotContain("Recargo:", textoLinea);

        await page.CloseAsync();
    }

    // Batch 2: negrita en las 4 variables (Cliente/Tipo Comprobante/Fecha/Forma de pago), sin el
    // text-muted que antes competia visualmente con el bold.
    [Fact]
    public async Task DetalleVenta_VariablesEnNegritaSinGrisQueLasAtenue()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=1755", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        // Bug real encontrado verificando este mismo test: la clase "font-weight-bold" (Bootstrap
        // 4) no existe en el bootstrap.min.css real de este proyecto (Bootstrap 5 la renombro a
        // "fw-bold") -- por eso el fix usa <strong> (negrita real del navegador) en vez de esa
        // clase, y el <h5> contenedor se fija en font-weight:400 explicito para que la etiqueta
        // quede genuinamente mas liviana que el valor.
        var resumen = page.Locator(".detalle-venta-resumen");
        var pesosValores = await resumen.Locator("h5 strong").EvaluateAllAsync<string[]>(
            "els => els.map(el => getComputedStyle(el).fontWeight)");
        Assert.Equal(4, pesosValores.Length);
        var pesoEtiquetas = await resumen.Locator("h5").First.EvaluateAsync<string>("el => getComputedStyle(el).fontWeight");
        var pesoEtiquetasNum = int.Parse(pesoEtiquetas == "normal" ? "400" : pesoEtiquetas);

        foreach (var peso in pesosValores)
        {
            var numerico = int.Parse(peso == "bold" ? "700" : peso);
            Assert.True(numerico >= 700, $"Se esperaba negrita real (>=700) en el valor, se obtuvo {peso}");
            Assert.True(numerico > pesoEtiquetasNum, $"El valor ({numerico}) debería ser más pesado que la etiqueta ({pesoEtiquetasNum}).");
        }

        // Ninguno de los 4 valores debe seguir teniendo text-muted (competia con el bold).
        Assert.Equal(0, await resumen.Locator("span.text-muted").CountAsync());

        await page.CloseAsync();
    }

    // Batch 3: el total y el importe de la ultima linea deben terminar en el mismo borde derecho
    // (con 1px de tolerancia por redondeo de layout).
    [Fact]
    public async Task DetalleVenta_TotalAlineadoConElBordeDerechoDeLasLineas()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=1755", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var lineaBox = await page.Locator(".venta-lineas .text-right").First.BoundingBoxAsync();
        var totalBox = await page.Locator(".card-body .text-right.mt-3").BoundingBoxAsync();
        Assert.NotNull(lineaBox);
        Assert.NotNull(totalBox);

        var bordeLinea = lineaBox!.X + lineaBox.Width;
        var bordeTotal = totalBox!.X + totalBox.Width;
        Assert.True(System.Math.Abs(bordeLinea - bordeTotal) <= 1,
            $"El total (borde derecho {bordeTotal}) debería terminar en el mismo margen que las líneas (borde derecho {bordeLinea}).");

        await page.CloseAsync();
    }

    // Batch 4: el boton "Volver" siempre aparece, incluso sin returnUrl (fallback al listado
    // general), y respeta el returnUrl real cuando llega desde el listado con un filtro puesto.
    [Fact]
    public async Task DetalleVenta_BotonVolver_SiempreApareceYRespetaElFiltroDeOrigen()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // Caso 1: acceso directo, sin returnUrl -- el boton igual aparece, apunta al listado general.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=1755", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(300);
        Assert.Equal(1, await page.Locator("#btnVolverListadoVentasDetalle").CountAsync());
        Assert.Equal("/Ventas", await page.GetAttributeAsync("#btnVolverListadoVentasDetalle", "href"));

        // Caso 2: viniendo del listado con un filtro de fechas puesto -- el boton vuelve a ESE
        // filtro. Simula el link real que _TablaVentas.cshtml arma (URL actual completa como
        // returnUrl), sin depender de que haya ventas reales en ese rango exacto.
        var returnUrl = System.Uri.EscapeDataString("/Ventas?fechaDesde=2022-09-10T00%3A00&fechaHasta=2026-09-10T23%3A59");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=1755&returnUrl={returnUrl}", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(300);
        var href = await page.GetAttributeAsync("#btnVolverListadoVentasDetalle", "href");
        Assert.Contains("fechaDesde=2022-09-10", href);

        await page.CloseAsync();
    }

    // Batch 4, gap real cerrado: entrar a DetalleVenta desde "Ver líneas" (antes no pasaba
    // returnUrl en absoluto, asi que el boton "Volver" no aparecia).
    [Fact]
    public async Task DetalleVenta_DesdeVerLineas_BotonVolverApuntaALineas()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/Lineas", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(800);

        var link = page.Locator("a:has-text('Ver registro')").First;
        if (await link.CountAsync() == 0)
        {
            // Sin ventas en el rango default de esta pantalla -- no hay nada que verificar hoy.
            await page.CloseAsync();
            return;
        }

        await link.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.Load);
        await page.WaitForTimeoutAsync(300);

        Assert.Equal(1, await page.Locator("#btnVolverListadoVentasDetalle").CountAsync());
        var href = await page.GetAttributeAsync("#btnVolverListadoVentasDetalle", "href");
        Assert.Contains("/Ventas/Lineas", href);

        await page.CloseAsync();
    }
}
