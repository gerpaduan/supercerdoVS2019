using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 1 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "Batch 9: Factura/
// Imprimir en DetalleVenta/DetalleFactura"): "agregar las opciones que faltan y estan en clasico
// 'factura' 'imprimir'. que se muestre el modal de Imprimir venta como en clasico (pero
// manteniendo el estilo que esta usando webcore)". Nuevo modal _ModalComprobanteVenta.cshtml
// (derivado de _ModalPostVentaBasico.cshtml, ya usado en Ventas/POS.cshtml) incluido una sola
// vez en _DetalleVentaCard.cshtml -- compartido automaticamente por DetalleVenta.cshtml y
// DetalleFactura.cshtml (ambas embeben esa misma partial sin duplicarla).
//
// Bug real encontrado y corregido durante este batch: _ModalComprobanteVenta.cshtml se renderiza
// dentro de @RenderBody(), que en _Layout.cshtml corre ANTES que jQuery se cargue (al final del
// <body>) -- cargar factura-electronica.js/detalle-venta-comprobante.js con un <script src>
// directo tiraba "ReferenceError: $ is not defined" (mismo patron de bug ya documentado para
// Personas/Index.cshtml). Fix: inyeccion diferida de esos 2 scripts via DOMContentLoaded.
//
// Venta de prueba real: #2008 (empresa 1, con datos reales en la base de dev). Factura de
// prueba real: id=166 (idventa=1755). Ambas son solo consultadas (Imprimir/Email preparan datos
// sin modificar la venta), no destructivas.
[Collection("WebCore browser")]
public sealed class DetalleVentaComprobanteTests
{
    private readonly WebCoreFixture _fixture;

    public DetalleVentaComprobanteTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DetalleVenta_BotonAbreModalConLosCuatroBotonesYSinErroresDeConsola()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add("[console] " + msg.Text); };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=2008", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        var boton = page.Locator("button:has-text('Factura / Imprimir')");
        Assert.Equal(1, await boton.CountAsync());

        await boton.ClickAsync();
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(1, await page.Locator("#modalComprobanteVenta.show").CountAsync());
        Assert.Equal(1, await page.Locator("#btnCvTicket:visible").CountAsync());
        Assert.Equal(1, await page.Locator("#btnCvImprimir:visible").CountAsync());
        Assert.Equal(1, await page.Locator("#btnCvEmail:visible").CountAsync());
        Assert.Equal(1, await page.Locator("#btnCvFactura:visible").CountAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DetalleVenta_ImprimirPdf_GeneraElPdfReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add("[console] " + msg.Text); };

        IResponse? respuestaImprimir = null;
        page.Context.Response += (_, resp) =>
        {
            if (resp.Url.Contains("/Ventas/Imprimir")) respuestaImprimir = resp;
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=2008", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);
        await page.ClickAsync("button:has-text('Factura / Imprimir')");
        await page.WaitForTimeoutAsync(500);

        await page.RunAndWaitForPopupAsync(async () => await page.ClickAsync("#btnCvImprimir"));
        await page.WaitForTimeoutAsync(1500);

        Assert.NotNull(respuestaImprimir);
        Assert.Equal(200, respuestaImprimir!.Status);
        var contentType = await respuestaImprimir.HeaderValueAsync("content-type");
        Assert.Equal("application/pdf", contentType);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DetalleVenta_ImprimirTicket_GeneraElTicketReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add("[console] " + msg.Text); };

        IResponse? respuestaTicket = null;
        page.Context.Response += (_, resp) =>
        {
            if (resp.Url.Contains("/Ventas/ImprimirTicketHtml")) respuestaTicket = resp;
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=2008", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);
        await page.ClickAsync("button:has-text('Factura / Imprimir')");
        await page.WaitForTimeoutAsync(500);

        await page.ClickAsync("#btnCvTicket");
        await page.WaitForTimeoutAsync(500);

        // Primera vez (sin tamaño recordado en localStorage): aparece un SweetAlert para elegir
        // 58/80mm -- confirmar con el default (80mm) antes de que se abra el ticket.
        var swalVisible = await page.Locator(".swal2-popup:visible").CountAsync();
        if (swalVisible > 0)
        {
            await page.RunAndWaitForPopupAsync(async () => await page.ClickAsync(".swal2-confirm"));
        }
        await page.WaitForTimeoutAsync(1500);

        Assert.NotNull(respuestaTicket);
        Assert.Equal(200, respuestaTicket!.Status);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DetalleVenta_Email_PreparaDatosYAbreElFormulario()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add("[console] " + msg.Text); };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleVenta?id=2008", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);
        await page.ClickAsync("button:has-text('Factura / Imprimir')");
        await page.WaitForTimeoutAsync(500);

        await page.ClickAsync("#btnCvEmail");
        await page.WaitForTimeoutAsync(800);

        Assert.Equal(1, await page.Locator("#modalEmailComprobanteVenta.show").CountAsync());
        Assert.Equal(1, await page.Locator("#cvEmailAsunto:visible").CountAsync());
        var asunto = await page.InputValueAsync("#cvEmailAsunto");
        Assert.False(string.IsNullOrWhiteSpace(asunto), "El asunto deberia venir pre-completado desde ObtenerDatosEmailComprobante.");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // Confirma que la partial compartida (_DetalleVentaCard.cshtml -> _ModalComprobanteVenta.cshtml)
    // funciona igual desde DetalleFactura.cshtml, sin duplicar wiring.
    [Fact]
    public async Task DetalleFactura_HeredaElMismoBotonYModal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        page.Console += (_, msg) => { if (msg.Type == "error") errors.Add("[console] " + msg.Text); };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/DetalleFactura?id=166", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        var boton = page.Locator("button:has-text('Factura / Imprimir')");
        Assert.Equal(1, await boton.CountAsync());

        await boton.ClickAsync();
        await page.WaitForTimeoutAsync(500);
        Assert.Equal(1, await page.Locator("#modalComprobanteVenta.show").CountAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
