using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal de Expendios de Ventas/POS, reorganizado por grupo de expendio (2026-09-06, pedido
// explicito del usuario -- ver docs/DECISIONS.md): una fila de encabezado por expendio (fecha,
// hora, nro, identificacion, sector, vendedor, UN solo boton "Cargar") y debajo una fila por cada
// item (producto + cantidad, sin boton propio). Este test tambien cubre la validacion pedida
// explicitamente: cargar un expendio agrega TODAS sus lineas, y "Quitar expendios cargados" solo
// quita las lineas que vinieron de un expendio -- una linea de producto agregada a mano antes de
// tocar expendios debe sobrevivir intacta.
[Collection("WebCore browser")]
public sealed class ExpendiosPOSTests
{
    private readonly WebCoreFixture _fixture;

    public ExpendiosPOSTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_ExpendiosAgrupados_CargaTodasLasLineasYQuitarNoBorraLineaManual()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // Linea manual, agregada ANTES de tocar expendios -- debe sobrevivir a "Quitar expendios".
        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(1, await page.Locator("#tablaItems tr.fila-item").CountAsync());

        await page.Keyboard.PressAsync("PageDown");
        await page.WaitForTimeoutAsync(800);
        Assert.Equal(1, await page.Locator("#modalExpendiosPOS.show").CountAsync());

        // Se deja el filtro por defecto ("Pendientes", ver resetFiltros() en
        // ventas-expendios-pos.js) a proposito -- NO se cambia a "Todos": bajo "Todos" pueden
        // aparecer expendios ya asignados a otra venta real (con el boton "Cargar" legitimamente
        // disabled), y esta base de desarrollo compartida ya tiene alguno en ese estado por
        // pruebas anteriores. "Pendientes" garantiza que todo lo que aparece es realmente
        // cargable, que es lo que este test necesita ejercitar.
        await page.WaitForTimeoutAsync(400);

        var botonHabilitado = page.Locator("#tablaExpendiosPOS tr.exp-group-header .btnCargarExpendioPOS:not([disabled])").First;
        if (await botonHabilitado.CountAsync() == 0)
        {
            // No hay expendios de prueba pendientes cargados en esta base -- no hay nada real
            // para ejercitar.
            await page.CloseAsync();
            return;
        }

        // El boton "Cargar" vive UNICAMENTE en la fila de encabezado, nunca en una fila de item
        // (puede haber mas de un grupo en la tabla -- se verifica en total, no por grupo).
        var totalHeaders = await page.Locator("#tablaExpendiosPOS tr.exp-group-header").CountAsync();
        Assert.Equal(totalHeaders, await page.Locator("#tablaExpendiosPOS tr.exp-group-header .btnCargarExpendioPOS").CountAsync());
        Assert.Equal(0, await page.Locator("#tablaExpendiosPOS tr.exp-group-item .btnCargarExpendioPOS").CountAsync());

        // Se carga el primer expendio REALMENTE cargable -- las lineas de item que le
        // corresponden a EL (por data-id-expendio), no el total de la tabla (puede haber mas de
        // un grupo visible).
        var idPrimerExpendio = await botonHabilitado.GetAttributeAsync("data-id-expendio");
        var filasItemAntes = await page.Locator("#tablaExpendiosPOS tr.exp-group-item[data-id-expendio='" + idPrimerExpendio + "']").CountAsync();
        Assert.True(filasItemAntes > 0, "El expendio de prueba no tiene lineas de item para verificar.");

        await botonHabilitado.ClickAsync();
        await page.WaitForTimeoutAsync(1200);

        if (await page.Locator("#modalObservacionesExpendio.show").CountAsync() > 0)
        {
            await page.Keyboard.PressAsync("Enter");
            await page.WaitForTimeoutAsync(500);
        }

        // Se cargaron TODAS las lineas del expendio (no solo una) mas la linea manual preexistente.
        Assert.Equal(1 + filasItemAntes, await page.Locator("#tablaItems tr.fila-item").CountAsync());

        await page.Keyboard.PressAsync("PageDown");
        await page.WaitForTimeoutAsync(600);
        await page.ClickAsync("#btnQuitarExpendiosPOS");
        await page.WaitForTimeoutAsync(400);
        if (await page.Locator(".swal2-confirm").CountAsync() > 0)
        {
            await page.ClickAsync(".swal2-confirm");
        }
        await page.WaitForTimeoutAsync(800);

        // "Quitar expendios cargados" borra SOLO las lineas del expendio -- la linea manual
        // preexistente sigue en el carrito (no se borra el carrito entero).
        Assert.Equal(1, await page.Locator("#tablaItems tr.fila-item").CountAsync());
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Precio/Total por item en el modal, y etiqueta "Cambio precio" en el carrito (2026-09-06,
    // pedido explicito del usuario -- ver docs/DECISIONS.md). El precio de un expendio queda
    // congelado al momento de crearlo en Puntos de Expendio (esa pantalla no aplica forma de
    // pago); VentasController.ObtenerExpendioPOS compara ese precio contra el precio de lista
    // ACTUAL del producto (join en vivo contra corte) y devuelve `cambioPrecio`. Los datos reales
    // de esta base no tienen hoy ninguna diferencia de precio -- se simula una linea con
    // cambioPrecio=true directamente sobre POSState para verificar que el render de la etiqueta
    // funciona, sin depender de mutar precios reales en la base compartida de desarrollo.
    [Fact]
    public async Task VentasPOS_ModalMuestraPrecioYTotalPorItem_YCarritoMuestraEtiquetaCambioPrecio()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync("PageDown");
        await page.WaitForTimeoutAsync(800);
        await page.SelectOptionAsync("#expFiltroEstado", "Todos");
        await page.WaitForTimeoutAsync(400);

        var grupos = await page.Locator("#tablaExpendiosPOS tr.exp-group-header").CountAsync();
        if (grupos == 0)
        {
            await page.CloseAsync();
            return;
        }

        var primerItemHtml = await page.Locator("#tablaExpendiosPOS tr.exp-group-item").First.InnerHTMLAsync();
        Assert.Contains("$", primerItemHtml);
        // Precio y Total son 2 celdas "text-right $ ..." distintas ademas de la de Cantidad.
        var celdasConSigno = System.Text.RegularExpressions.Regex.Matches(primerItemHtml, @"text-right"">\$").Count;
        Assert.True(celdasConSigno >= 2, "El modal deberia mostrar Precio y Total (2 celdas con '$') por item, HTML: " + primerItemHtml);

        var badgePresente = await page.EvaluateAsync<bool>(@"() => {
            window.POSState.addLinea({
                index: window.POSState.nextIndex(),
                idCorte: 999, idExpendio: 999, producto: 'PRODUCTO PRUEBA CAMBIO PRECIO',
                descripcion: 'PRODUCTO PRUEBA', codigo: 999,
                cant: '1.000', precio: '$ 100.00', precioOriginal: '$ 100.00',
                subtotal: '$ 100.00', bonificacion: 0, anulado: false, indexAnulado: -1,
                balanza: false, pesable: true, cambioPrecio: true, precioListaActual: 150
            });
            window.renderTablaProductos(window.POSState.getLineas());
            return document.getElementById('tablaItems').innerHTML.includes('Cambio precio');
        }");

        Assert.True(badgePresente, "El carrito deberia mostrar la etiqueta 'Cambio precio' cuando la linea tiene cambioPrecio=true.");
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
