using System.Globalization;
using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Exportar Ventas > Lineas a PDF y Excel (2026-10-13, ver docs/DECISIONS.md "Exportar Lineas de venta a
// PDF y Excel"). Ejercita las dos acciones nuevas (VentasController.LineasPdf / LineasExcel) y los botones
// de la vista, que arman la URL con los filtros "en vivo" (producto, cliente, vendedor, formas de pago) y las
// fechas aplicadas. Requiere WebCore corriendo y el login de dev (WebCoreFixture).
[Collection("WebCore browser")]
public sealed class ExportarLineasVentaTests
{
    private const string FormatoFechaQuery = "yyyy-MM-ddTHH:mm:ss";
    private const string TipoXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly WebCoreFixture _fixture;

    public ExportarLineasVentaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    // Rango de hoy (00:00 a 23:59:59): puede no tener ventas, a proposito -- los endpoints tienen que devolver un
    // archivo valido igual (el documento dice que no hay ventas).
    private static string QueryHoy()
    {
        var hoy = DateTime.Today;
        return "fechaDesde=" + hoy.ToString(FormatoFechaQuery, CultureInfo.InvariantCulture)
            + "&fechaHasta=" + hoy.AddDays(1).AddSeconds(-1).ToString(FormatoFechaQuery, CultureInfo.InvariantCulture)
            + "&idSucursal=-1";
    }

    // Ultimos 30 dias: rango acotado donde es probable que haya ventas, sin cargar todo el historial.
    private static string UrlLineasUltimos30Dias()
    {
        var hoy = DateTime.Today;
        return $"{WebCoreFixture.BaseUrl}/Ventas/Lineas?fechaDesde=" + hoy.AddDays(-30).ToString(FormatoFechaQuery, CultureInfo.InvariantCulture)
            + "&fechaHasta=" + hoy.AddDays(1).AddSeconds(-1).ToString(FormatoFechaQuery, CultureInfo.InvariantCulture)
            + "&idSucursal=-1";
    }

    [Fact]
    public async Task Endpoints_DevuelvenPdfYXlsxValidos()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        // Mismas cookies de sesion que la pagina logueada.
        var pdf = await page.Context.APIRequest.GetAsync($"{WebCoreFixture.BaseUrl}/Ventas/LineasPdf?{QueryHoy()}");
        Assert.True(pdf.Ok, "LineasPdf respondio " + pdf.Status);
        Assert.Equal("application/pdf", pdf.Headers["content-type"]);
        Assert.Matches(@"filename=""?Lineas_de_venta_\d{8}_\d{4}\.pdf", pdf.Headers["content-disposition"]);
        var bytesPdf = await pdf.BodyAsync();
        Assert.True(bytesPdf.Length > 1000, "El PDF es sospechosamente chico.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytesPdf, 0, 4));

        var excel = await page.Context.APIRequest.GetAsync($"{WebCoreFixture.BaseUrl}/Ventas/LineasExcel?{QueryHoy()}");
        Assert.True(excel.Ok, "LineasExcel respondio " + excel.Status);
        Assert.Equal(TipoXlsx, excel.Headers["content-type"]);
        Assert.Matches(@"filename=""?Lineas_de_venta_\d{8}_\d{4}\.xlsx", excel.Headers["content-disposition"]);
        var bytesExcel = await excel.BodyAsync();
        Assert.True(bytesExcel.Length > 1000, "El xlsx es sospechosamente chico.");
        // Un .xlsx es un zip: empieza con "PK".
        Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(bytesExcel, 0, 2));

        // Excel compacto: mismo contenido que el anterior en forma de impresion; "datos" son los opcionales elegidos.
        var compacto = await page.Context.APIRequest.GetAsync($"{WebCoreFixture.BaseUrl}/Ventas/LineasExcelCompacto?{QueryHoy()}&datos=vendedor%2Csucursal%2Ccliente");
        Assert.True(compacto.Ok, "LineasExcelCompacto respondio " + compacto.Status);
        Assert.Equal(TipoXlsx, compacto.Headers["content-type"]);
        Assert.Matches(@"filename=""?Lineas_de_venta_\d{8}_\d{4}_compacto\.xlsx", compacto.Headers["content-disposition"]);
        var bytesCompacto = await compacto.BodyAsync();
        Assert.True(bytesCompacto.Length > 1000, "El xlsx compacto es sospechosamente chico.");
        Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(bytesCompacto, 0, 2));

        // Un valor de "datos" desconocido se ignora (input externo): no rompe ni cambia el resultado.
        var compactoRaro = await page.Context.APIRequest.GetAsync($"{WebCoreFixture.BaseUrl}/Ventas/LineasExcelCompacto?{QueryHoy()}&datos=zzz%2C%3Cscript%3E");
        Assert.True(compactoRaro.Ok, "LineasExcelCompacto con datos desconocidos respondio " + compactoRaro.Status);

        await page.CloseAsync();
    }

    [Fact]
    public async Task ExcelCompacto_PideLosDatosYDescargaCuandoHayVentas()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync(UrlLineasUltimos30Dias(), new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        var botonCompacto = page.Locator("button.js-exportar-compacto");
        Assert.Equal(1, await botonCompacto.CountAsync());

        // Sin ninguna venta visible no hay ventana de datos: solo el aviso.
        await page.Locator("[data-live-filter='producto']").FillAsync("zzz-producto-que-no-existe-zzz");
        await botonCompacto.ClickAsync();
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Equal(0, await page.Locator(".swal-dato-compacto").CountAsync());
        await page.Locator(".swal2-confirm").ClickAsync();
        await page.Locator("[data-live-filter='producto']").FillAsync("");

        if (await page.Locator(".js-registro-card:not(.d-none)").CountAsync() == 0)
        {
            // Sin ventas en los ultimos 30 dias de la base de dev: nada que descargar hoy.
            await page.CloseAsync();
            return;
        }

        // Con ventas: la ventana ofrece las 6 casillas (vendedor y cliente tildados de entrada).
        await botonCompacto.ClickAsync();
        await page.Locator(".swal-dato-compacto").First.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Equal(6, await page.Locator(".swal-dato-compacto").CountAsync());

        var descarga = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.Locator(".swal2-confirm").ClickAsync();
        }, new PageRunAndWaitForDownloadOptions { Timeout = 60000 });
        Assert.Matches(@"^Lineas_de_venta_\d{8}_\d{4}_compacto\.xlsx$", descarga.SuggestedFilename);
        var ruta = Path.Combine(Path.GetTempPath(), "webcore-e2e-lineas-venta-compacto.xlsx");
        await descarga.SaveAsAsync(ruta);
        var bytes = await File.ReadAllBytesAsync(ruta);
        Assert.True(bytes.Length > 1000, "El xlsx compacto descargado es sospechosamente chico.");
        Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(bytes, 0, 2));
        File.Delete(ruta);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Botones_ArmanLaUrlConLosFiltrosEnVivo()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync(UrlLineasUltimos30Dias(), new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        var botonPdf = page.Locator("a.js-exportar-lineas:has-text('PDF')");
        var botonExcel = page.Locator("a.js-exportar-lineas:has-text('Excel')");
        Assert.Equal(1, await botonPdf.CountAsync());
        Assert.Equal(1, await botonExcel.CountAsync());

        // Filtros "en vivo": no tienen name, asi que solo viajan porque el script los agrega a la URL.
        await page.Locator("[data-live-filter='producto']").FillAsync("carne");
        await page.Locator("[data-live-filter='cliente']").FillAsync("consumidor");
        await page.Locator("[data-live-filter='vendedor']").FillAsync("ger");
        await page.Locator("[data-live-filter='formaPago'][value='Efectivo']").CheckAsync();
        await page.Locator("[data-live-filter='formaPago'][value='Qr']").CheckAsync();

        var hrefPdf = await botonPdf.GetAttributeAsync("href") ?? "";
        Assert.Contains("/Ventas/LineasPdf?", hrefPdf);
        Assert.Contains("producto=carne", hrefPdf);
        Assert.Contains("cliente=consumidor", hrefPdf);
        Assert.Contains("vendedor=ger", hrefPdf);
        Assert.Contains("formasPago=Efectivo%2CQr", hrefPdf);
        Assert.Contains("idSucursal=-1", hrefPdf);
        Assert.Contains("fechaDesde=", hrefPdf);
        Assert.Contains("fechaHasta=", hrefPdf);

        var hrefExcel = await botonExcel.GetAttributeAsync("href") ?? "";
        Assert.Contains("/Ventas/LineasExcel?", hrefExcel);
        Assert.Contains("producto=carne", hrefExcel);

        await page.CloseAsync();
    }

    [Fact]
    public async Task Botones_SinVentasVisiblesNoDescargan()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync(UrlLineasUltimos30Dias(), new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        // Un filtro en vivo que no coincide con nada deja la pantalla sin tarjetas visibles.
        await page.Locator("[data-live-filter='producto']").FillAsync("zzz-producto-que-no-existe-zzz");
        Assert.Equal(0, await page.Locator(".js-registro-card:not(.d-none)").CountAsync());

        // El click se cancela (aviso) y no se descarga nada: la descarga esperada vence por timeout.
        await Assert.ThrowsAnyAsync<PlaywrightException>(() => page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.ClickAsync("a.js-exportar-lineas:has-text('PDF')");
        }, new PageRunAndWaitForDownloadOptions { Timeout = 1500 }));

        await page.CloseAsync();
    }

    [Fact]
    public async Task Botones_DescargaPdfYExcelCuandoHayVentas()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync(UrlLineasUltimos30Dias(), new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        if (await page.Locator(".js-registro-card:not(.d-none)").CountAsync() == 0)
        {
            // Sin ventas en los ultimos 30 dias de la base de dev: nada que exportar hoy (los endpoints se
            // cubren en Endpoints_DevuelvenPdfYXlsxValidos).
            await page.CloseAsync();
            return;
        }

        var descargaPdf = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.ClickAsync("a.js-exportar-lineas:has-text('PDF')");
        }, new PageRunAndWaitForDownloadOptions { Timeout = 60000 });
        Assert.Matches(@"^Lineas_de_venta_\d{8}_\d{4}\.pdf$", descargaPdf.SuggestedFilename);
        var rutaPdf = Path.Combine(Path.GetTempPath(), "webcore-e2e-lineas-venta.pdf");
        await descargaPdf.SaveAsAsync(rutaPdf);
        var bytesPdf = await File.ReadAllBytesAsync(rutaPdf);
        Assert.True(bytesPdf.Length > 1000, "El PDF descargado es sospechosamente chico.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytesPdf, 0, 4));
        File.Delete(rutaPdf);

        var descargaExcel = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.ClickAsync("a.js-exportar-lineas:has-text('Excel')");
        }, new PageRunAndWaitForDownloadOptions { Timeout = 60000 });
        Assert.Matches(@"^Lineas_de_venta_\d{8}_\d{4}\.xlsx$", descargaExcel.SuggestedFilename);
        var rutaExcel = Path.Combine(Path.GetTempPath(), "webcore-e2e-lineas-venta.xlsx");
        await descargaExcel.SaveAsAsync(rutaExcel);
        var bytesExcel = await File.ReadAllBytesAsync(rutaExcel);
        Assert.True(bytesExcel.Length > 1000, "El xlsx descargado es sospechosamente chico.");
        Assert.Equal("PK", System.Text.Encoding.ASCII.GetString(bytesExcel, 0, 2));
        File.Delete(rutaExcel);

        // La pantalla sigue en pie: bajar el archivo no recarga la pagina.
        Assert.Contains("/Ventas/Lineas", page.Url);

        await page.CloseAsync();
    }
}
