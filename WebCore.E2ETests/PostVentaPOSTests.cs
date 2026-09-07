using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal de post-venta: atajos numericos 1-5, ticket termico HTML (58/80mm, con memoria de tamaño)
// y PDF con eleccion de tipo de comprobante (2026-09-06, pedido explicito del usuario -- ver
// docs/DECISIONS.md). Orden de botones/atajos: 1=Nueva venta, 2=Ticket, 3=PDF, 4=Email,
// 5=Factura electronica.
[Collection("WebCore browser")]
public sealed class PostVentaPOSTests
{
    private readonly WebCoreFixture _fixture;

    public PostVentaPOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task FinalizarVentaSimpleAsync(IPage page)
    {
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
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1200);

        await page.Locator("#modalPostVentaBasico.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
    }

    [Fact]
    public async Task PostVenta_BotonesNumeradosEnOrden()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await FinalizarVentaSimpleAsync(page);

        var botones = new[] { "#btnPvbContinuar", "#btnPvbTicket", "#btnPvbImprimir", "#btnPvbEmail", "#btnPvbFactura" };
        for (int i = 0; i < botones.Length; i++)
        {
            Assert.Equal((i + 1).ToString(), (await page.Locator(botones[i] + " .badge").InnerTextAsync()).Trim());
        }

        await page.CloseAsync();
    }

    [Fact]
    public async Task PostVenta_AtajoNumerico2_AbreSelectorDeTamañoYLuegoTicket()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await FinalizarVentaSimpleAsync(page);

        // Sin tamaño recordado todavia (localStorage limpio en este contexto de browser nuevo) --
        // el atajo "2" debe abrir el selector de 58/80mm antes de imprimir.
        await page.Keyboard.PressAsync("2");
        await page.WaitForTimeoutAsync(400);
        Assert.True(await page.Locator(".swal2-popup").IsVisibleAsync());
        Assert.Contains("Tamaño de ticket", await page.Locator(".swal2-title").InnerTextAsync());

        var popupTask = page.WaitForPopupAsync();
        await page.ClickAsync(".swal2-confirm");
        var popup = await popupTask;
        await popup.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        Assert.Contains("ImprimirTicketHtml", popup.Url);
        Assert.Contains("mm=80", popup.Url);
        await popup.CloseAsync();

        // El tamaño recien elegido queda recordado y visible en el modal.
        Assert.Equal("80mm", (await page.Locator("#pvbTicketMedidaTexto").InnerTextAsync()).Trim());

        await page.CloseAsync();
    }

    [Fact]
    public async Task PostVenta_TamañoDeTicketRecordado_NoVuelveAPreguntar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync("localStorage.setItem('postventa_ticket_mm', '58')");

        await FinalizarVentaSimpleAsync(page);
        Assert.Equal("58mm", (await page.Locator("#pvbTicketMedidaTexto").InnerTextAsync()).Trim());

        var popupTask = page.WaitForPopupAsync();
        await page.Keyboard.PressAsync("2");
        var popup = await popupTask;
        await popup.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        Assert.Contains("mm=58", popup.Url);
        await popup.CloseAsync();

        await page.CloseAsync();
    }

    [Fact]
    public async Task PostVenta_AtajoNumerico3_SinFacturaAbrePdfDetalleDirecto()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await FinalizarVentaSimpleAsync(page);

        // Venta recien creada, sin factura asociada -- igual que el clasico (pvSeleccionarOpcionSimple
        // solo pregunta si hay factura), debe abrir el PDF de detalle directo sin dialogo.
        // El PDF vuelve con Content-Disposition:attachment (File(bytes, "application/pdf", nombre)
        // en el controller) -- Chromium headless lo trata como descarga, no como navegacion normal
        // de la pestaña nueva, asi que se espera el evento de descarga en vez de una URL de pagina.
        var download = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.Keyboard.PressAsync("3");
        });
        Assert.Contains("/Ventas/Imprimir", download.Url);
        Assert.Contains("documento=detalle", download.Url);

        await page.CloseAsync();
    }
}
