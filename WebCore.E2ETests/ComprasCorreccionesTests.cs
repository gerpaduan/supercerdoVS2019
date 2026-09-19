using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Correcciones 2026-09-19 (ver docs/DECISIONS.md "Compras: lector, alta de producto en iframe, precio
// monetario, Cta. Cte. en modal"): Compras/Editar (lector de codigo de barra, alta completa de producto
// para un EAN inexistente, precio monetario, codigo pendiente bloquea el guardado), Cta. Cte. de
// proveedor (abrir una Compra en modal origen=modal), AddOrEditPago (Nro de recibo editable) y POS
// (foco en #inputCodigo al cerrar el modal de linea). Los tests que ESCRIBEN en la base de desarrollo
// (guardar una compra / un producto) solo corren con E2E_ALLOW_WRITES=1.
[Collection("WebCore browser")]
public sealed class ComprasCorreccionesTests
{
    // EAN-13 valido (dígito verificador correcto) que no existe en la base de desarrollo.
    private const string EanInexistente = "7790000000003";

    private static bool EscriturasPermitidas => Environment.GetEnvironmentVariable("E2E_ALLOW_WRITES") == "1";

    private readonly WebCoreFixture _fixture;

    public ComprasCorreccionesTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirNuevaCompraAsync(IPage page)
    {
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Compras/NuevaCompra", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);
        return page;
    }

    // Elige un proveedor con el buscador F9 (primer resultado) y devuelve su id (#idPersona).
    private static async Task<int> ElegirProveedorAsync(IPage page)
    {
        await page.Keyboard.PressAsync("F9");
        await page.Locator("#modalBuscarPersona.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.FillAsync("#filtroPersona", "a");
        await page.WaitForTimeoutAsync(700);
        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(500);
        return int.Parse(await page.Locator("#idPersona").InputValueAsync());
    }

    [Fact]
    public async Task Compras_PrecioMonetario_PuntoEsComaDecimalYFormateaMiles()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);
        await AbrirNuevaCompraAsync(page);

        await page.ClickAsync("#txtPrecioKg");
        await page.Keyboard.TypeAsync("1234.5");

        Assert.Equal("1.234,5", await page.Locator("#txtPrecioKg").InputValueAsync());
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_PrecioMonetario_SubtotalUsaElValorSinPuntosDeMiles()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);

        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(800);
        await page.FillAsync("#txtCantKgs", "2");
        await page.ClickAsync("#txtPrecioKg");
        await page.Keyboard.TypeAsync("1234.5");
        await page.WaitForTimeoutAsync(300);

        // 2 x 1234,50 = 2469,00 (si se leyera "1.234,5" con toNumber crudo daria 2 x 1,234).
        Assert.Equal("2469.00", await page.Locator("#txtSubtotalLinea").InputValueAsync());
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_CodigoSinAgregar_BloqueaElGuardadoConAviso()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);

        await page.FillAsync("#txtCodigoProducto", "12345");
        await page.WaitForTimeoutAsync(400);
        await page.ClickAsync("#btnGuardarCompra");

        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Código", await page.Locator(".swal2-popup").InnerTextAsync());
        Assert.Contains("/Compras/Editar", page.Url);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_TieneBotonDeEscanerCompartido()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);

        Assert.True(await page.Locator("#btnCompraScanner").IsVisibleAsync());
        Assert.False(await page.Locator("#compraScannerContainer").IsVisibleAsync());
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_EanInexistente_OfreceAltaYAbreLaVistaRealDeProductosConElCodigo()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        var pedidos = new List<string>();
        page.Request += (_, req) => pedidos.Add(req.Url);
        page.PageError += (_, msg) => errors.Add(msg);
        await AbrirNuevaCompraAsync(page);

        await page.FillAsync("#txtCodigoProducto", EanInexistente);
        await page.Keyboard.PressAsync("Enter");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Producto inexistente", await page.Locator(".swal2-popup").InnerTextAsync());
        await page.ClickAsync(".swal2-confirm");

        await page.Locator("#modalAltaProductoCompra.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        var frame = page.FrameLocator("#frmAltaProductoCompra");
        await frame.Locator("#Codigo").WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
        Assert.Equal(EanInexistente, await frame.Locator("#Codigo").InputValueAsync());

        // Layout minimo: sin sidebar ni carga continua (no se puede "seguir cargando" dentro del modal).
        Assert.Equal(0, await frame.Locator("#accordionSidebar").CountAsync());
        Assert.Equal(0, await frame.Locator("#swCargaContinua").CountAsync());

        // El codigo precargado dispara el mismo autocompletado desde el catalogo global que el tipeo manual
        // (la espera cubre el debounce y a que el script del iframe enganche sus handlers).
        await page.WaitForTimeoutAsync(1500);
        Assert.Contains(pedidos, url => url.Contains("BuscarProductoGlobalParaAlta") && url.Contains(EanInexistente));

        // "Volver" dentro del iframe solo cierra el modal.
        await frame.Locator("#btnVolverProducto").ClickAsync();
        await page.Locator("#modalAltaProductoCompra.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 5000 });
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_MensajeProductoGuardado_AgregaElProductoALaCompra()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);

        // Simula el postMessage que emite Productos/EmbedGuardado (codigo 1 existe en la base de desarrollo).
        await page.EvaluateAsync("window.postMessage({ tipo: 'producto-guardado', codigo: 1 }, window.location.origin)");
        await page.WaitForTimeoutAsync(800);

        Assert.False(string.IsNullOrWhiteSpace(await page.Locator("#txtProductoNombre").InputValueAsync()));
        Assert.Equal("1", await page.Locator("#txtCodigoProducto").InputValueAsync());
        await page.CloseAsync();
    }

    [Fact]
    public async Task Compras_MensajeSinCodigo_SeIgnora()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);

        // Mismo tipo de mensaje pero con forma invalida (sin codigo): no debe tocar el formulario.
        await page.EvaluateAsync("window.postMessage({ tipo: 'producto-guardado' }, window.location.origin)");
        await page.WaitForTimeoutAsync(500);

        Assert.Equal("", await page.Locator("#txtProductoNombre").InputValueAsync());
        await page.CloseAsync();
    }

    [Fact]
    public async Task Pago_NroDeRecibo_EsEditable()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);
        var idPersona = await ElegirProveedorAsync(page);
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/AddOrEditPago?idPersona={idPersona}", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });

        var nroRecibo = page.Locator("#NroRecibo");
        Assert.False(string.IsNullOrWhiteSpace(await nroRecibo.InputValueAsync()));
        Assert.True(await nroRecibo.IsEditableAsync());

        await nroRecibo.FillAsync("MANUAL-0001");
        Assert.Equal("MANUAL-0001", await nroRecibo.InputValueAsync());
        await page.CloseAsync();
    }

    [Theory]
    [InlineData("/Ventas/POS")]
    [InlineData("/PuntosExpendio/POS")]
    public async Task POS_AlCerrarElModalDeLinea_ElFocoVuelveAlInputCodigo(string ruta)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{ruta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var sectorModal = page.Locator("#modalSectoresPuntoExpendio");
        if (await sectorModal.IsVisibleAsync())
        {
            await sectorModal.Locator("button, a").First.ClickAsync();
            await page.WaitForTimeoutAsync(500);
        }

        await page.FillAsync("#inputCodigo", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);
        await page.FillAsync("#inputCantidad", "2");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(600);

        // Cierre por la X del modal (todas las formas de cierre pasan por el mismo hidden.bs.modal).
        await page.ClickAsync("#tablaItems tr.fila-item");
        await page.Locator("#modalLineaVenta.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        await page.ClickAsync("#modalLineaVenta .close");
        await page.Locator("#modalLineaVenta.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 5000 });
        await page.WaitForTimeoutAsync(900);
        Assert.Equal("inputCodigo", await page.EvaluateAsync<string>("document.activeElement && document.activeElement.id"));

        await page.CloseAsync();
    }

    // Cta. Cte. embebida en POS: el modal de compra debe apilarse ENCIMA de #modalFinanzasPOS y el buscador de
    // producto de la compra encima de ambos. Solo lectura (abre y cierra, no guarda). Si el proveedor elegido
    // no tiene compras en la Cta. Cte. no hay nada que abrir y el test termina sin verificar (base de desarrollo).
    [Fact]
    public async Task CtaCte_EmbebidaEnPOS_ElModalDeCompraYSuBuscadorSeApilanEncima()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);
        var idProveedor = await ElegirProveedorAsync(page);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);
        var desde = DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd");
        await page.EvaluateAsync($"window.POSFinanzas.cargar('/Finanzas/CtaCtePersona?idPersona={idProveedor}&fechaDesde={desde}&desdePos=true', 'Cuenta corriente')");
        await page.Locator("#modalFinanzasPOS.show tr.js-fila-ctacte, #modalFinanzasPOS.show #tablaPersona").First.WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });

        var filasCompra = page.Locator("#modalFinanzasPOS tr.js-fila-ctacte[data-es-compra='true']");
        if (await filasCompra.CountAsync() == 0) return;

        await filasCompra.Last.ClickAsync();
        await page.Locator("#modalCompraCtaCte.show #formCompra").WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
        await page.WaitForTimeoutAsync(600);

        // El centro del modal de compra tiene que ser (parte de) el modal de compra, no el de POS que esta detras.
        Assert.True(await page.EvaluateAsync<bool>(@"(() => {
            const r = document.querySelector('#modalCompraCtaCte .modal-content').getBoundingClientRect();
            const el = document.elementFromPoint(r.left + r.width / 2, r.top + 80);
            return !!el && !!el.closest('#modalCompraCtaCte');
        })()"));

        // Buscador de producto (F10): queda por encima del modal de compra (la compra abre en solo lectura: "Modificar").
        await page.ClickAsync("#modalCompraCtaCte #btnHabilitarEdicionCompra");
        await page.ClickAsync("#modalCompraCtaCte #btnBuscarProducto");
        await page.Locator("#modalBuscarProductoCompra.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(600);
        Assert.True(await page.EvaluateAsync<bool>(@"(() => {
            const r = document.querySelector('#modalBuscarProductoCompra .modal-content').getBoundingClientRect();
            const el = document.elementFromPoint(r.left + r.width / 2, r.top + 40);
            return !!el && !!el.closest('#modalBuscarProductoCompra');
        })()"));
        await page.CloseAsync();
    }

    // ---- Kgs.Medias (Stock/Pesaje): el punto es decimal, nunca miles ----------------------------------

    [Fact]
    public async Task Stock_KgsMedias_LaComaTipeadaSeConvierteEnPuntoDecimal()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo?tipoCompra=Pesaje Cortes", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForTimeoutAsync(800);

        await page.FillAsync("#KgsMedias", "");
        await page.ClickAsync("#KgsMedias");
        await page.Keyboard.TypeAsync("1267,5");

        Assert.Equal("1267.5", await page.Locator("#KgsMedias").InputValueAsync());
        await page.CloseAsync();
    }

    // Antes del fix, "1267.5" se leia en es-AR como 12675 (punto = miles). Crea un pesaje nuevo con Kgs.Medias
    // tipeado con coma y verifica que se guarda sin error. El valor guardado (Compras.kgsMedias, entero en
    // Postgres: 1267,5 -> 1268) se comprobo ademas con una consulta directa a la base de desarrollo.
    [Fact]
    public async Task Stock_KgsMedias_PesajeNuevoSeGuardaSinMultiplicarPorCien()
    {
        if (!EscriturasPermitidas) return;

        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Stock/Nuevo?tipoCompra=Pesaje Cortes", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        await page.WaitForTimeoutAsync(800);
        await page.FillAsync("#KgsMedias", "");
        await page.ClickAsync("#KgsMedias");
        await page.Keyboard.TypeAsync("1267,5");
        Assert.Equal("1267.5", await page.Locator("#KgsMedias").InputValueAsync());
        await page.FillAsync("#CantMedias", "2");
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(900);
        await page.FillAsync("#txtCantKgs", "1");
        await page.ClickAsync("#btnAgregarLineaStock");
        await page.WaitForTimeoutAsync(500);
        await page.ClickAsync("#btnGuardarStock");
        await page.WaitForURLAsync(u => u.Contains("/Stock") && !u.Contains("/Stock/Nuevo") && !u.Contains("/Stock/Guardar"), new PageWaitForURLOptions { Timeout = 15000 });
        Assert.Equal(0, await page.Locator(".alert-danger:visible").CountAsync());
        await page.CloseAsync();
    }

    // ---- Tests que ESCRIBEN en la base de desarrollo (E2E_ALLOW_WRITES=1) --------------------------

    // Compra Media Res de 1267 kg: Compras.kgsMedias tiene que guardarse como 1267 (antes: 126700, porque el hidden
    // #KgsMedias viaja como "1267.00" y el binder es-AR lo leia con punto de miles). Solo corre si la empresa admite
    // Media Res (selector "Tipo compra" visible). El valor guardado se comprobo con una consulta directa a la base.
    [Fact]
    public async Task Compras_MediaRes_KgsMediasSeGuardaSinMultiplicarPorCien()
    {
        if (!EscriturasPermitidas) return;

        var page = await _fixture.NewAuthenticatedPageAsync();
        await AbrirNuevaCompraAsync(page);
        if (await page.Locator("select#TipoCompra").CountAsync() == 0)
        {
            await page.CloseAsync();
            return;
        }

        await page.SelectOptionAsync("select#TipoCompra", new SelectOptionValue { Label = "Media" });
        await ElegirProveedorAsync(page);
        await page.FillAsync("#NroRemito", "E2E-MEDIA-" + DateTime.Now.ToString("HHmmss"));
        await page.FillAsync("#txtKgMedia", "1267");
        await page.FillAsync("#txtPrecioMedia", "10");
        await page.ClickAsync("#btnAgregarLineaMediaRes");
        await page.WaitForTimeoutAsync(500);
        Assert.Equal("1267.00", await page.Locator("#KgsMedias").InputValueAsync());
        await page.FillAsync("#CantMediasMediaRes", "1");
        await page.ClickAsync("#btnGuardarCompra");
        await page.WaitForURLAsync(url => url.Contains("/Compras") && !url.Contains("/Editar"), new PageWaitForURLOptions { Timeout = 15000 });
        await page.CloseAsync();
    }

    private static string GenerarEan13Unico()
    {
        // 779 + 9 digitos derivados del reloj + digito verificador EAN-13.
        var cuerpo = "779" + (DateTime.Now.Ticks % 1_000_000_000L).ToString("D9");
        var suma = 0;
        for (var i = 0; i < 12; i++)
            suma += (cuerpo[i] - '0') * (i % 2 == 0 ? 1 : 3);
        return cuerpo + ((10 - (suma % 10)) % 10);
    }

    [Fact]
    public async Task Compras_AltaProductoEnIframe_GuardaYAgregaElProductoALaCompra()
    {
        if (!EscriturasPermitidas) return;

        var page = await _fixture.NewAuthenticatedPageAsync();
        var ean = GenerarEan13Unico();
        var nombre = "ZZ E2E ALTA " + DateTime.Now.ToString("HHmmss");
        await AbrirNuevaCompraAsync(page);

        await page.FillAsync("#txtCodigoProducto", ean);
        await page.Keyboard.PressAsync("Enter");
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        await page.ClickAsync(".swal2-confirm");
        await page.Locator("#modalAltaProductoCompra.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });

        var frame = page.FrameLocator("#frmAltaProductoCompra");
        await frame.Locator("#CorteDesc").WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
        await page.WaitForTimeoutAsync(1500);
        await frame.Locator("#CorteDesc").FillAsync(nombre);
        await frame.Locator("#PrecioKg").FillAsync("100");
        await frame.Locator("#ddlTipo").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await frame.Locator("#ddlIva").SelectOptionAsync(new SelectOptionValue { Index = 1 });
        await frame.Locator("#btnGuardarProducto").ClickAsync();

        // El iframe avisa por postMessage: el modal se cierra y la linea queda lista con el producto nuevo.
        await page.Locator("#modalAltaProductoCompra.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached, Timeout = 15000 });
        await page.WaitForTimeoutAsync(800);
        Assert.Equal(nombre, await page.Locator("#txtProductoNombre").InputValueAsync());
        Assert.Equal(ean, await page.Locator("#txtCodigoProducto").InputValueAsync());
        await page.CloseAsync();
    }

    [Fact]
    public async Task CtaCte_CompraSeAbreEnModal_GuardaSinCajaYRecargaLaCuentaCorriente()
    {
        if (!EscriturasPermitidas) return;

        var page = await _fixture.NewAuthenticatedPageAsync();
        var nroRemito = "E2E-MODAL-" + DateTime.Now.ToString("HHmmss");

        // 1) Compra nueva (pantalla completa) para el proveedor, en cuenta corriente, para tener una fila.
        await AbrirNuevaCompraAsync(page);
        var idProveedor = await ElegirProveedorAsync(page);
        await page.FillAsync("#NroRemito", nroRemito);
        await page.CheckAsync("#EnCtaCte");
        await page.FillAsync("#txtCodigoProducto", "1");
        await page.Keyboard.PressAsync("Enter");
        await page.WaitForTimeoutAsync(800);
        await page.FillAsync("#txtCantKgs", "1");
        await page.FillAsync("#txtPrecioKg", "1");
        await page.ClickAsync("#btnAgregarLineaCorte");
        await page.WaitForTimeoutAsync(500);
        await page.ClickAsync("#btnGuardarCompra");
        await page.WaitForURLAsync(url => url.Contains("/Compras") && !url.Contains("/Editar"), new PageWaitForURLOptions { Timeout = 15000 });

        // 2) Cta. Cte. del proveedor: la fila de la compra abre el modal.
        var desde = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Finanzas/CtaCtePersona?idPersona={idProveedor}&fechaDesde={desde}", new PageGotoOptions { WaitUntil = WaitUntilState.Load });
        var filaCompra = page.Locator("tr.js-fila-ctacte[data-es-compra='true']").Last;
        await filaCompra.ClickAsync();
        await page.Locator("#modalCompraCtaCte.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.Locator("#modalCompraCtaCte #formCompra").WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });
        Assert.Equal(nroRemito, await page.Locator("#modalCompraCtaCte #NroRemito").InputValueAsync());

        // 3) Editar y guardar: cierra el modal y la cuenta corriente se recarga sola con aviso.
        await page.ClickAsync("#modalCompraCtaCte #btnHabilitarEdicionCompra");
        await page.FillAsync("#modalCompraCtaCte #Observaciones", "editada desde cta cte (E2E)");
        var recarga = page.WaitForNavigationAsync(new PageWaitForNavigationOptions { Timeout = 15000 });
        await page.ClickAsync("#modalCompraCtaCte #btnGuardarCompra");
        await recarga;
        await page.Locator(".swal2-popup").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Compra guardada", await page.Locator(".swal2-popup").InnerTextAsync());
        Assert.Contains("/Finanzas/CtaCtePersona", page.Url);
        await page.CloseAsync();
    }
}
