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

    // Item 2 de la cuarta ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "Batch 8: rediseño
    // modal Forma de Pago"): el bug real reportado no era que el modal se desbordara del viewport
    // (eso ya lo cubre modal-dialog-scrollable, test de arriba) -- era que, DENTRO del modal ya
    // visible, aparecia un SCROLL INTERNO molesto con controles fuera de la vista al desplegar el
    // bloque de descuento. Este test verifica eso especificamente: el modal-body no debe necesitar
    // scroll interno (scrollHeight <= clientHeight) con el bloque desplegado, en un viewport
    // desktop realista (1366x720, no el 1920x1080 generoso del test de arriba).
    [Fact]
    public async Task VentasPOS_ModalFormaPago_ConDescuentoDesplegado_SinScrollInternoEnViewportDesktopRealista()
    {
        var page = await _fixture.NewAuthenticatedPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1366, Height = 720 }
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

        var medidas = await page.EvaluateAsync<double[]>(
            "() => { var el = document.querySelector('#modalFormaPago .modal-body'); return [el.scrollHeight, el.clientHeight]; }");
        Assert.True(medidas[0] <= medidas[1] + 1,
            $"El modal-body tiene scroll interno (scrollHeight={medidas[0]}, clientHeight={medidas[1]}) -- ¿se saco pos-modal-compacto de _FormaPagoModal.cshtml/forma-pago.js?");

        await page.CloseAsync();
    }

    // Mismo item: "reducir la altura de los botones... al activar, y al desactivar que vuelvan al
    // tamaño normal". Compara la altura real de un boton de forma de pago antes de abrir el
    // bloque de descuento, con el bloque abierto (debe ser estrictamente menor), y tras cerrarlo
    // de nuevo (debe volver a la altura original).
    [Fact]
    public async Task VentasPOS_ModalFormaPago_BotonesSeCompactanAlAbrirDescuentoYVuelvenAlCerrar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
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

        var botonEfectivo = page.Locator("#modalFormaPago .btn-forma-pago[data-tipo='Efectivo']");
        var alturaNormal = (await botonEfectivo.BoundingBoxAsync())!.Height;

        await page.ClickAsync("#btnTogglePorcentajeTotalVenta");
        await page.WaitForTimeoutAsync(400);
        Assert.True(await page.Locator("#modalFormaPago").EvaluateAsync<bool>("el => el.classList.contains('pos-modal-compacto')"));
        var alturaCompacta = (await botonEfectivo.BoundingBoxAsync())!.Height;
        Assert.True(alturaCompacta < alturaNormal,
            $"El boton deberia achicarse con el bloque de descuento abierto (normal={alturaNormal}px, compacto={alturaCompacta}px).");

        // Cerrar con el mismo atajo "/" (toggleBloquePorcentajeTotalVenta) -- vuelve al tamaño normal.
        await page.ClickAsync("#btnTogglePorcentajeTotalVenta");
        await page.WaitForTimeoutAsync(400);
        Assert.False(await page.Locator("#modalFormaPago").EvaluateAsync<bool>("el => el.classList.contains('pos-modal-compacto')"));
        var alturaTrasCerrar = (await botonEfectivo.BoundingBoxAsync())!.Height;
        Assert.True(System.Math.Abs(alturaTrasCerrar - alturaNormal) < 2,
            $"El boton deberia volver a su altura normal al cerrar el bloque (esperado ~{alturaNormal}px, obtuvo {alturaTrasCerrar}px).");

        await page.CloseAsync();
    }

    // Checkbox -> interruptor para "Pago Mixto" (mismo item): cambio puramente visual, cero
    // cambios de funcionamiento -- mismo id/evento change. Este test no existia para el checkbox
    // viejo; confirma que el bloque de montos sigue mostrandose/ocultandose igual con el switch.
    [Fact]
    public async Task VentasPOS_ModalFormaPago_SwitchPagoMixto_MuestraYOcultaElBloqueDeMontos()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
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

        Assert.Equal(1, await page.Locator("#modalFormaPago .custom-switch #chkPagoMixto").CountAsync());
        Assert.False(await page.Locator("#bloquePagoMixto").IsVisibleAsync());

        await page.ClickAsync("label[for='chkPagoMixto']");
        // slideDown()/slideUp() sin duracion explicita usan el default de jQuery (400ms) --
        // esperar mas que eso para no leer el estado a mitad de la animacion.
        await page.WaitForTimeoutAsync(600);
        Assert.True(await page.Locator("#bloquePagoMixto").IsVisibleAsync());
        Assert.True(await page.IsCheckedAsync("#chkPagoMixto"));

        await page.ClickAsync("label[for='chkPagoMixto']");
        await page.WaitForTimeoutAsync(600);
        Assert.False(await page.Locator("#bloquePagoMixto").IsVisibleAsync());

        await page.CloseAsync();
    }

    // Etiqueta visible del atajo "/" (mismo item): antes solo estaba como title (tooltip on-hover).
    [Fact]
    public async Task VentasPOS_ModalFormaPago_EtiquetaDeAtajoDescuentoEsVisible()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
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

        var etiqueta = page.Locator("#lblAtajoDescuentoTotalVenta");
        Assert.True(await etiqueta.IsVisibleAsync());
        Assert.Contains("/", await etiqueta.InnerTextAsync());

        await page.CloseAsync();
    }
}
