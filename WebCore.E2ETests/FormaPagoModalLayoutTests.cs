using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Item 4 de los 12 pendientes (2026-09-09, ver docs/DECISIONS.md "Batch C: POS forma de pago"):
// "corregir, que los botones de las diferentes formas de pago queden alineados y proporcionados".
// Causa raiz: la vista usa la clase "btn-block" (Bootstrap 4, width:100%) pero el proyecto carga
// Bootstrap 5, que eliminó esa clase -- sin un reemplazo, cada boton quedaba con ancho de
// contenido (chico), desalineado dentro de su columna .col-4. Fix en
// wwwroot/Content/css/custom.css (#modalFormaPago .btn-forma-pago { width:100%; ... }). Este test
// verifica la geometria real (bounding boxes), no solo la presencia de la clase CSS.
[Collection("WebCore browser")]
public sealed class FormaPagoModalLayoutTests
{
    private readonly WebCoreFixture _fixture;

    public FormaPagoModalLayoutTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_ModalFormaPago_BotonesOcupanTodoElAnchoDeSuColumna()
    {
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(600);
        Assert.Equal(1, await page.Locator("#modalFormaPago.show").CountAsync());

        var botones = page.Locator("#modalFormaPago .btn-forma-pago");
        var cantidad = await botones.CountAsync();
        Assert.Equal(6, cantidad);

        var anchos = new List<double>();
        for (var i = 0; i < cantidad; i++)
        {
            var box = await botones.Nth(i).BoundingBoxAsync();
            Assert.NotNull(box);
            anchos.Add(box!.Width);
        }

        // Los 6 botones estan en una grilla de 3 columnas iguales (.col-4) -- deben medir todos
        // practicamente lo mismo (tolerancia de 2px por redondeo de layout), no un ancho de
        // contenido variable segun el largo del texto ("Débito" vs "Cta. Cte.").
        var referencia = anchos[0];
        foreach (var ancho in anchos)
        {
            Assert.True(System.Math.Abs(ancho - referencia) < 2,
                $"Los botones de forma de pago deberian medir todos igual (~{referencia}px), pero uno midio {ancho}px -- ver custom.css #modalFormaPago .btn-forma-pago.");
        }

        // Deben ocupar la mayor parte del ancho de su columna (antes del fix, el boton quedaba
        // angosto -- ancho de contenido -- dejando un hueco vacio a la derecha dentro de la
        // misma .col-4). Con 3 columnas en un modal modal-lg (~800px de contenido), cada columna
        // mide bastante mas de 150px -- si el fix se rompe, el ancho vuelve a ~90-120px de
        // contenido fijo sin importar el viewport.
        Assert.True(referencia > 150, $"Los botones de forma de pago parecen angostos ({referencia}px) -- ¿volvio el bug de 'btn-block' muerto en Bootstrap 5?");

        await page.CloseAsync();
    }

    // El bloque de descuento/recargo % a todos los productos (#bloquePorcentajeTotalVenta, mejora
    // propia de WebCore sin equivalente en clasico) podia empujar el contenido del modal fuera de
    // la pantalla en viewports bajos. Fix: modal-dialog-scrollable en _FormaPagoModal.cshtml.
    // Verificado en un viewport bajo (700px de alto) real: el modal completo debe quedar dentro
    // del viewport (con scroll interno si hace falta), nunca desbordar hacia abajo del viewport.
    [Fact]
    public async Task VentasPOS_ModalFormaPago_ConDescuentoDesplegado_QuedaDentroDelViewport()
    {
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1024, Height = 700 }
        });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(600);
        await page.ClickAsync("#btnTogglePorcentajeTotalVenta");
        await page.WaitForTimeoutAsync(400);
        Assert.True(await page.Locator("#bloquePorcentajeTotalVenta").IsVisibleAsync());

        var modalDialogBox = await page.Locator("#modalFormaPago .modal-dialog").BoundingBoxAsync();
        Assert.NotNull(modalDialogBox);
        Assert.True(modalDialogBox!.Y >= 0, "El modal-dialog no deberia arrancar arriba del viewport.");
        Assert.True(modalDialogBox!.Y + modalDialogBox!.Height <= 700 + 1,
            $"El modal-dialog se desborda del viewport (bottom={modalDialogBox!.Y + modalDialogBox!.Height}px, viewport=700px) -- ¿se saco modal-dialog-scrollable de _FormaPagoModal.cshtml?");

        await page.CloseAsync();
    }
}
