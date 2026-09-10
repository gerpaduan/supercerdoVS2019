using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Modal de Factura Electronica (AFIP), 2026-09-06 (retomado -- ver docs/DECISIONS.md). Port
// literal de Web/Views/Ventas/_FacturaElectronica.cshtml + Scripts/app/factura-electronica.js,
// wireado desde el boton "Factura electronica" del post-venta basico (_ModalPostVentaBasico.cshtml)
// via abrirFacturaVentaModal (POS.cshtml). Usa una venta real ya existente en la base local (id
// 1755) en vez de completar una venta nueva por UI, para no acoplar este test al flujo completo
// del POS -- el mismo criterio que otros tests de este proyecto que usan datos de seed conocidos.
[Collection("WebCore browser")]
public sealed class FacturaElectronicaTests
{
    private readonly WebCoreFixture _fixture;

    public FacturaElectronicaTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_AbreModalFacturaDesdePostVenta_YCierraSinFacturar()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // Simula el post-venta de una venta real ya existente -- window.mostrarModalPostVenta ya
        // esta expuesto globalmente por POS.cshtml (lo usa el flujo real tras FinalizarVenta).
        await page.EvaluateAsync("window.mostrarModalPostVenta(1755)");
        await page.WaitForTimeoutAsync(300);

        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());
        Assert.True(await page.Locator("#btnPvbFactura").IsVisibleAsync());

        await page.ClickAsync("#btnPvbFactura");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.False(string.IsNullOrWhiteSpace(await page.Locator("#feStickyCliente").InnerTextAsync()));
        Assert.NotEqual("-", (await page.Locator("#feStickyCliente").InnerTextAsync()).Trim());
        Assert.True(await page.Locator("#btnCerrarVentaSinFacturar").IsVisibleAsync());

        // Cerrar sin facturar es una operacion segura (no toca AFIP) -- confirma el SweetAlert y
        // verifica que vuelve al post-venta basico (mismo comportamiento que el clasico).
        await page.ClickAsync("#btnCerrarVentaSinFacturar");
        await page.WaitForTimeoutAsync(400);
        await page.ClickAsync(".swal2-confirm");
        await page.WaitForTimeoutAsync(1200);

        Assert.Equal(0, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "modal Factura Electronica:
    // bloque blanco"): en el flujo "Nueva factura sin venta" (esSinVenta=true, unico lugar donde
    // esto pasa), _FacturaElectronica.cshtml inyecta #modalBuscarPersona ANIDADO dentro de
    // #contenedorFacturaElectronica (que tiene overflow:hidden) en vez de como hijo directo de
    // <body> -- el patron estandar de Bootstrap para modales. Con custom.css forzando el mismo
    // z-index fijo para TODOS los modales (sin incrementar por anidamiento), el backdrop del 2do
    // modal terminaba tapando mal el contenido del primero. Fix: factura-electronica.js mueve el
    // nodo a <body> antes de mostrarlo, y lo saca de ahi al cerrarse.
    [Fact]
    public async Task VentasPOS_NuevaFacturaSinVenta_ModalBuscarPersonaNoQuedaAnidadoYEsInteractuable()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.EvaluateAsync("window.VentasFacturaModal.abrirSinVenta()");
        await page.WaitForTimeoutAsync(1000);
        Assert.Equal(1, await page.Locator("#modalFacturaElectronica.show").CountAsync());

        await page.ClickAsync("#btnFeBuscarCliente");
        await page.WaitForTimeoutAsync(800);
        Assert.Equal(1, await page.Locator("#modalBuscarPersona.show").CountAsync());

        // Assert principal: ya no queda anidado dentro del modal de factura -- antes del fix,
        // esto era el origen del problema de stacking/backdrop.
        var anidado = await page.EvaluateAsync<bool>(
            "() => { var el = document.getElementById('modalBuscarPersona'); var cont = document.getElementById('contenedorFacturaElectronica'); return !!el && !!cont && cont.contains(el); }");
        Assert.False(anidado, "#modalBuscarPersona no deberia quedar anidado dentro de #contenedorFacturaElectronica");

        // Interactuable de punta a punta: filtrar, seleccionar, confirmar que se propaga al
        // formulario de factura.
        await page.FillAsync("#filtroPersona", "a");
        await page.WaitForTimeoutAsync(600);
        Assert.True(await page.Locator("#tablaPersonas tr.fila-persona").CountAsync() > 0);

        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(600);

        Assert.Equal(0, await page.Locator("#modalBuscarPersona.show").CountAsync());
        Assert.NotEqual("0", await page.InputValueAsync("#feIdPersonaSeleccionada"));
        Assert.Empty(errors);

        // Bug real, encontrado verificando visualmente el fix de arriba con capturas reales
        // (2026-09-10, ver docs/DECISIONS.md): con top:-.2rem, al quedar el formulario
        // scrolleado (seleccionar un cliente dispara un scroll), el bloque sticky de resumen
        // (#feStickyResumen) se pegaba mas arriba de lo que llegaba a cubrir su propio fondo
        // opaco, dejando un resto sin tapar de la card "Emisor" (justo arriba) visible y
        // amontonado contra el -- exactamente el "bloque blanco que quita vision a los otros
        // componentes" reportado. El fix real fue top:1rem (mas cobertura del fondo opaco), no
        // separar los rectangulos -- geometricamente siguen solapando un poco, pero ya no se ve
        // texto de la card Emisor "sangrando" a traves del sticky. Assert mecanico: en el centro
        // del bloque sticky, el elemento que realmente pinta ahi (elementFromPoint) es el sticky
        // en si (o un hijo suyo), no la card Emisor -- confirma que el fondo opaco lo cubre.
        await page.WaitForTimeoutAsync(500);
        var stickyBox = await page.Locator("#feStickyResumen").BoundingBoxAsync();
        Assert.NotNull(stickyBox);
        var centroX = stickyBox!.X + stickyBox.Width / 2;
        var centroY = stickyBox.Y + stickyBox.Height / 2;
        var elementoEncima = await page.EvaluateAsync<string>(
            $"() => {{ var el = document.elementFromPoint({centroX}, {centroY}); return el ? (el.closest('.fe-sticky-resumen') ? 'sticky' : el.closest('.fe-card-compact') ? 'emisor' : el.tagName) : 'ninguno'; }}");
        Assert.Equal("sticky", elementoEncima);

        await page.CloseAsync();
    }

    // Bug real reportado por el usuario 2026-09-09 (ver docs/DECISIONS.md): "cuando se seleccionaba
    // la forma de pago que no sea efectivo ni ctacte, siempre iba directo al modal de factura
    // electronica. y asi debe ser". Causa raiz: forma-pago.js:728-730 ya llamaba a
    // window.VentasFacturaModal.requiereFacturaAutomatica(payload.formaPago) al finalizar una venta
    // para decidir si abre la Factura Electronica directo en vez del modal basico de post-venta,
    // pero window.VentasFacturaModal (definido en POS.cshtml) nunca definio ese metodo (ni
    // empresaPuedeFacturar) -- la condicion siempre daba false/undefined y caia siempre al modal
    // basico. Fix: se agregaron ambos metodos a POS.cshtml (port literal de
    // Web/Scripts/app/modal-postventa.js:10-28), leyendo window.POSFacturaElectronicaConfig (nuevo,
    // seteado por VentasController.POS con VentasController.EmpresaTieneCertificadoFacturaElectronica).
    [Fact]
    public async Task VentasPOS_FinalizarConFormaPagoNoEfectivo_AbreFacturaElectronicaDirecto()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        Assert.True(await page.EvaluateAsync<bool>(
            "window.POSFacturaElectronicaConfig && window.POSFacturaElectronicaConfig.empresaTieneCertificado === true"),
            "Este test asume que la empresa de 'ger' en la base de dev tiene certificado AFIP cargado -- si cambia, ajustar el usuario de prueba.");

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        await page.Keyboard.PressAsync("End");
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Transferencia']");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.Equal(0, await page.Locator("#modalPostVentaBasico.show").CountAsync());

        await page.CloseAsync();
    }

    // Caso negativo, mismo bug: confirma que Efectivo (que nunca debe auto-facturar, ni en clasico)
    // sigue yendo al modal basico -- no alcanza con que el nuevo camino ande, tiene que seguir sin
    // afectar el camino que ya andaba bien.
    [Fact]
    public async Task VentasPOS_FinalizarConEfectivo_NoAbreFacturaElectronicaAutomatica()
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
        await page.WaitForTimeoutAsync(800);
        await page.ClickAsync("button.btn-forma-pago[data-tipo='Efectivo']");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(0, await page.Locator("#modalFacturaElectronica.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalPostVentaBasico.show").CountAsync());

        await page.CloseAsync();
    }
}
