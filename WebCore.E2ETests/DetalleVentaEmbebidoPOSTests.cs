using Microsoft.Playwright;

namespace WebCore.E2ETests;

// 2 bugs reales reportados por el usuario 2026-09-12 (ver docs/DECISIONS.md "Batch 1"), ambos en
// el Detalle de Venta cuando se ve embebido dentro de POS (via "Mis actividades" -> "Mis ventas"
// -> ver detalle, cargado en #modalFinanzasPOS con window.POSVentas.abrirDetalle).
[Collection("WebCore browser")]
public sealed class DetalleVentaEmbebidoPOSTests
{
    private readonly WebCoreFixture _fixture;

    public DetalleVentaEmbebidoPOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(IPage page, string idVenta)> CrearVentaYVerDetalleEmbebidoAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (await page.Locator("#modalSucursales.show").CountAsync() > 0)
            await page.Locator("#lnkContinuarSucursalActual").ClickAsync();
        await page.WaitForTimeoutAsync(500);

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

        // Vuelve a POS (nueva venta) y abre el detalle embebido de la venta recien creada, mismo
        // mecanismo real que "Mis actividades" -> "Mis ventas" -> click en una fila.
        await page.ClickAsync("#btnPvbContinuar");
        await page.WaitForTimeoutAsync(800);
        await page.EvaluateAsync("(id) => window.POSVentas.abrirDetalle(id)", idVenta);
        await page.Locator("#modalFinanzasPOS.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(600);

        return (page, idVenta);
    }

    [Fact]
    public async Task ModificarVenta_DesdeDetalleEmbebido_NoDisparaCartelDeSalida()
    {
        var (page, _) = await CrearVentaYVerDetalleEmbebidoAsync(_fixture);

        var dialogsDisparados = new List<string>();
        page.Dialog += (_, dialog) =>
        {
            dialogsDisparados.Add(dialog.Message);
            _ = dialog.DismissAsync();
        };

        var btnModificar = page.Locator("#btnModificarVentaDetalle");
        Assert.True(await btnModificar.CountAsync() > 0, "no aparecio el boton 'Modificar venta' -- verificar que el usuario de prueba tenga permiso");

        await btnModificar.ClickAsync();
        // El bug real: sin el fix, esto dispara el beforeunload nativo -- si el test llega a
        // navegar hasta /Ventas/POS?idVentaEditar=X sin que page.Dialog haya capturado nada,
        // confirma que el aviso no se disparo.
        await page.WaitForURLAsync(u => u.ToString().Contains("idVentaEditar"), new PageWaitForURLOptions { Timeout = 15000 });

        Assert.Empty(dialogsDisparados);

        await page.CloseAsync();
    }

    [Fact]
    public async Task FacturaImprimir_DesdeDetalleEmbebido_AbreVisibleSinQuedarTrabado()
    {
        var (page, _) = await CrearVentaYVerDetalleEmbebidoAsync(_fixture);

        await page.ClickAsync("#modalFinanzasPOS button:has-text('Factura / Imprimir')");
        await page.Locator("#modalComprobanteVenta.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(400);

        // El bug real: sin el fix, el data-api de Bootstrap ocultaba #modalFinanzasPOS (el fondo)
        // al abrir #modalComprobanteVenta (data-bs-toggle plano) -- como #modalComprobanteVenta es
        // descendiente del DOM de #modalFinanzasPOS, ocultar el ancestro lo colapsaba a 0x0
        // (invisible) aunque el propio modal tuviera la clase "show". Se verifica visibilidad e
        // interactividad real, no solo la clase, y que #modalFinanzasPOS siga abierto de fondo.
        Assert.True(await page.Locator("#modalComprobanteVenta").IsVisibleAsync(), "el modal de comprobante deberia ser visible, no quedar oculto detras de #modalFinanzasPOS");
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());

        var botonCerrar = page.Locator("#modalComprobanteVenta [data-bs-dismiss='modal']").First;
        Assert.True(await botonCerrar.CountAsync() > 0);
        await botonCerrar.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
        await page.Locator("#modalComprobanteVenta.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });

        // Tras cerrar, el Detalle de venta (de fondo) sigue interactuable -- confirma que no quedo
        // un backdrop huerfano bloqueando todo.
        Assert.True(await page.Locator("#modalFinanzasPOS").IsVisibleAsync());

        await page.CloseAsync();
    }
}
