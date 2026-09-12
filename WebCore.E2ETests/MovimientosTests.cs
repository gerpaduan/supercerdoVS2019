using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modulo Movimientos (traslados de stock entre sucursales), no incluido en el plan original de
// migracion en 8 modulos -- se ataco en una sesion aparte (ver docs/10-migracion-aspnet-core/
// README.md). Prueba real de punta a punta: alta de un movimiento nuevo con click + submit real
// (antiforgery real, sin bypass), igual que GenerarEtiquetasPdfTests para Productos.
[Collection("WebCore browser")]
public sealed class MovimientosTests
{
    private readonly WebCoreFixture _fixture;

    public MovimientosTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task NuevoMovimiento_AgregaLineaYGuardaReal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        // Filtra en origen el quirk heredado del original (Web/Views/Movimientos/Editar.cshtml
        // usa el mismo type="number" en #txtCodigoProducto/#txtCantUnidad): movimientos.js llama
        // a setSelectionRange/.select() sobre esos inputs, y los navegadores no lo permiten para
        // type=number -- error real en consola, presente identico en Web clasico (mismo JS,
        // mismo markup, dispara varias veces durante el flujo normal). No se corrige aca
        // (CLAUDE.md §5, fuera de alcance de esta migracion).
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange")) errors.Add(msg);
        };

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        // Codigo real (CARRE, ya usado en el resto de la migracion) -- Enter dispara
        // BuscarProductoPorCodigo y completa el nombre del producto.
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Locator("#txtCodigoProducto").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtProductoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });

        var nombreProducto = await page.InputValueAsync("#txtProductoNombre");
        Assert.False(string.IsNullOrWhiteSpace(nombreProducto));

        await page.FillAsync("#txtCantUnidad", "1");
        await page.FillAsync("#txtCantKgs", "1,5");
        await page.ClickAsync("#btnAgregarProducto");

        // La linea se agrega a la tabla client-side (mismo mecanismo que el resto de esta
        // migracion para tablas de lineas: Stock/Compras/Ventas).
        await page.WaitForSelectorAsync("#tablaLineasMovimiento tbody tr", new PageWaitForSelectorOptions { Timeout = 5000 });
        Assert.Equal(1, await page.Locator("#tablaLineasMovimiento tbody tr").CountAsync());

        var marcador = "E2E movimiento " + DateTime.UtcNow.Ticks;
        await page.FillAsync("#Observaciones", marcador);

        await page.ClickAsync("#btnGuardarMovimiento");

        // Guardado real -> se abre el modal post-guardado (version reducida, sin ticket ESC/POS).
        await page.WaitForSelectorAsync("#modalPostMovimiento.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        Assert.Equal(1, await page.Locator("#modalPostMovimiento:visible").CountAsync());

        Assert.Empty(errors);

        // Cierra sin navegar (no hace falta limpiar: Movimientos no tiene accion de anulacion/borrado
        // en el original -- mismo criterio que las compras/ventas de prueba de este programa, quedan
        // marcadas en Observaciones, no se borran).
        await page.ClickAsync("#btnPostMovimientoNoImprimir");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("/Movimientos", page.Url);

        await page.CloseAsync();
    }

    // Bug real (2026-09-10, tercera ronda de pedidos, ver docs/DECISIONS.md): "habilitar los
    // botones de imprimir, tal como lo hace el clasico, en vistas como editar movimiento".
    // Causa: window.movimientosConfig.imprimirUrl quedaba SIEMPRE vacio (campo muerto -- el
    // agente de impresion local ESC/POS nunca se porto a WebCore, decision ya tomada), y
    // openPrintOptions() en movimientos.js gateaba con "!config.imprimirUrl" -- como era
    // siempre falsy, el boton nunca abria #modalPostMovimiento (que YA funciona bien para PDF/
    // WhatsApp, ninguno de los 2 depende de imprimirUrl). Fix: el guard ahora mira config.pdfUrl.
    [Fact]
    public async Task BtnImprimirMovimiento_AbreModalConPdfFuncional()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange")) errors.Add(msg);
        };

        // Crear un movimiento real primero (mismo flujo que NuevoMovimiento_AgregaLineaYGuardaReal)
        // para tener un id real de EsEdicion=true, unico escenario donde #btnImprimirMovimiento
        // se renderiza (Editar.cshtml:26-31).
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Locator("#txtCodigoProducto").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtProductoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });
        await page.FillAsync("#txtCantUnidad", "1");
        await page.FillAsync("#txtCantKgs", "1,5");
        await page.ClickAsync("#btnAgregarProducto");
        await page.WaitForSelectorAsync("#tablaLineasMovimiento tbody tr", new PageWaitForSelectorOptions { Timeout = 5000 });
        await page.FillAsync("#Observaciones", "E2E imprimir " + DateTime.UtcNow.Ticks);

        var guardarResponseTask = page.WaitForResponseAsync(r => r.Url.Contains("/Movimientos/Guardar") && r.Request.Method == "POST");
        await page.ClickAsync("#btnGuardarMovimiento");
        var guardarResponse = await guardarResponseTask;
        var body = await guardarResponse.JsonAsync();
        var pdfUrl = body!.Value.GetProperty("pdfUrl").GetString();
        Assert.False(string.IsNullOrWhiteSpace(pdfUrl), $"pdfUrl real: '{pdfUrl}' -- body completo: {body}");
        // ImprimirPdf usa ruta con segmento (/Movimientos/ImprimirPdf/{id}), no query string.
        var idMovimiento = pdfUrl!.TrimEnd('/').Split('/').Last();
        Assert.False(string.IsNullOrWhiteSpace(idMovimiento), $"No se pudo extraer el id de pdfUrl='{pdfUrl}'");

        await page.WaitForSelectorAsync("#modalPostMovimiento.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        await page.ClickAsync("#btnPostMovimientoNoImprimir");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Ahora el escenario real reportado: entrar a Editar/{id} y clickear "Imprimir".
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar/{idMovimiento}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(400);

        var swalCount = await page.Locator(".swal2-popup:visible").CountAsync();
        Assert.Equal(0, swalCount); // no deberia haber ningun warning previo

        await page.ClickAsync("#btnImprimirMovimiento");
        await page.WaitForTimeoutAsync(500);

        Assert.Equal(0, await page.Locator(".swal2-popup:visible").CountAsync());
        Assert.Equal(1, await page.Locator("#modalPostMovimiento.show").CountAsync());

        // #btnPostMovimientoPdf abre el PDF en una pestana nueva (abrirNuevaVentana) -- la
        // response real llega en el Page nuevo, no en el original, por eso se escucha a nivel
        // de BrowserContext (mismo patron ya usado en DetalleVentaComprobanteTests).
        IResponse? respuestaPdf = null;
        page.Context.Response += (_, resp) =>
        {
            if (resp.Url.Contains("/Movimientos/ImprimirPdf")) respuestaPdf = resp;
        };
        await page.RunAndWaitForPopupAsync(async () => await page.ClickAsync("#btnPostMovimientoPdf"));
        await page.WaitForTimeoutAsync(1000);

        Assert.NotNull(respuestaPdf);
        Assert.Equal(200, respuestaPdf!.Status);
        Assert.Equal("application/pdf", await respuestaPdf.HeaderValueAsync("content-type"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // Boton "Imprimir" nuevo en la columna Acciones del listado (item 1 de la cuarta ronda de
    // pedidos, 2026-09-10, ver docs/DECISIONS.md "Batch 7: boton Imprimir en Movimientos + ticket
    // termico"). A diferencia del test de arriba (que ejercita #btnImprimirMovimiento dentro de
    // Editar.cshtml), este cubre el botón NUEVO del listado (Index.cshtml), que trae sus datos via
    // ImprimirInfo (AJAX) recien al click -- no por fila, para no cargar entidades de mas.
    [Fact]
    public async Task BotonImprimirEnListado_AbreModalConTicketTermicoFuncional()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange")) errors.Add(msg);
        };

        // Crear un movimiento real primero (mismo patron que el test de arriba).
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Editar", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Locator("#txtCodigoProducto").PressAsync("Enter");
        await page.WaitForFunctionAsync("() => document.getElementById('txtProductoNombre').value.length > 0", new PageWaitForFunctionOptions { Timeout = 5000 });
        await page.FillAsync("#txtCantUnidad", "1");
        await page.FillAsync("#txtCantKgs", "1,5");
        await page.ClickAsync("#btnAgregarProducto");
        await page.WaitForSelectorAsync("#tablaLineasMovimiento tbody tr", new PageWaitForSelectorOptions { Timeout = 5000 });
        await page.FillAsync("#Observaciones", "E2E imprimir listado " + DateTime.UtcNow.Ticks);

        var guardarResponseTask = page.WaitForResponseAsync(r => r.Url.Contains("/Movimientos/Guardar") && r.Request.Method == "POST");
        await page.ClickAsync("#btnGuardarMovimiento");
        var guardarResponse = await guardarResponseTask;
        var body = await guardarResponse.JsonAsync();
        var movimientoId = body!.Value.GetProperty("movimientoId").GetInt32();

        await page.WaitForSelectorAsync("#modalPostMovimiento.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        await page.ClickAsync("#btnPostMovimientoNoImprimir");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        var fila = page.Locator($"tr[data-movimiento-row='{movimientoId}']");
        await fila.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        await fila.Locator(".js-imprimir-movimiento").ClickAsync();
        await page.Locator("#modalPostMovimiento.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        // WhatsApp oculto a pedido del usuario -- el boton sigue en el DOM, no visible.
        Assert.False(await page.Locator("#btnPostMovimientoWhatsapp").IsVisibleAsync());

        // Sin tamaño recordado todavia (localStorage limpio en este contexto de browser nuevo) --
        // el boton "Imprimir" (2) debe desplegar el selector de tamaño en vez de abrir directo.
        await page.ClickAsync("#btnPostMovimientoImprimir");
        await page.Locator("#bloqueTicketOpcionesMovimiento.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        var popup = await page.Context.RunAndWaitForPageAsync(async () =>
        {
            await page.ClickAsync("button.btnTicketMovimientoOpt[data-mm='80']");
        });
        await popup.WaitForLoadStateAsync(LoadState.Load);
        Assert.Contains("/Movimientos/ImprimirTicket/" + movimientoId, popup.Url);
        Assert.Contains("mm=80", popup.Url);
        var contenidoTicket = await popup.Locator("pre.ticket-text").InnerTextAsync();
        Assert.Contains("Movimiento", contenidoTicket);
        await popup.CloseAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // Bug real (2026-09-08, ver docs/DECISIONS.md): "/Movimientos/Nuevo" tiraba
    // InvalidOperationException. Causa: Nuevo() delega a Editar(0,...) por llamada C# directa
    // (no RedirectToAction); Editar() hacia "return View(model)" sin nombre explicito, y ASP.NET
    // Core resuelve el nombre de vista segun la RUTA ENTRANTE ("/Movimientos/Nuevo"), no segun el
    // metodo que corrio -- buscaba "Views/Movimientos/Nuevo.cshtml", que no existe. El unico test
    // existente (arriba) solo navegaba a "/Movimientos/Editar", nunca reprodujo el bug.
    [Fact]
    public async Task Nuevo_CargaSinError()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var pageErrors = new List<string>();
        // Mismo quirk heredado y ya documentado arriba (setSelectionRange sobre inputs
        // type="number") -- no es parte de este bug, se filtra igual que en el otro test.
        page.PageError += (_, msg) =>
        {
            if (!msg.Contains("setSelectionRange") && !msg.Contains("setSelection")) pageErrors.Add(msg);
        };

        var response = await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Movimientos/Nuevo", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 20000
        });

        Assert.NotNull(response);
        Assert.True(response!.Ok, $"GET /Movimientos/Nuevo devolvio {response.Status}");
        Assert.True(await page.Locator("#txtCodigoProducto").CountAsync() > 0, "no se renderizo el formulario de Movimientos (Views/Movimientos/Editar.cshtml)");
        Assert.Empty(pageErrors);

        await page.CloseAsync();
    }
}
