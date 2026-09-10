using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Regresion de los atajos F2/F5/F6/F7 del POS de Ventas y F6 del POS de PuntosExpendio,
// portados 2026-09-06 (retomado del plan de login/permisos reales -- ver docs/DECISIONS.md).
// Cada atajo abre el modal generico #modalFinanzasPOS con contenido real cargado por AJAX
// (renderConScripts + POSFinanzas/POSCompras/POSEgresos) -- este test verifica que la clase
// "show" se aplique de verdad (geometria real de Bootstrap, no solo presencia de markup) y que
// el contenido cargado no este vacio, para detectar si window.POSModalLoading o el AJAX se
// rompen en un cambio futuro.
[Collection("WebCore browser")]
public sealed class PosHotkeysTests
{
    private readonly WebCoreFixture _fixture;

    public PosHotkeysTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Theory]
    [InlineData("F2")]
    [InlineData("F5")]
    [InlineData("F6")]
    [InlineData("F7")]
    public async Task VentasPOS_Hotkey_AbreModalFinanzasConContenidoReal(string tecla)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        await page.Keyboard.PressAsync(tecla);
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        var contenido = await page.Locator("#contenedorFinanzasPOS").InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(contenido), $"Modal de {tecla} quedo sin contenido -- el AJAX pudo haber fallado.");
        Assert.DoesNotContain("Cargando", contenido);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    [Fact]
    public async Task PuntosExpendioPOS_F6_AbreModalMisExpendios()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/PuntosExpendio/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        // El modal de seleccion de sector se abre solo al entrar (sector obligatorio, ver
        // header de PuntosExpendio/POS.cshtml) -- el guard de abrirMisExpendios() en
        // punto-expendio-pos.js bloquea F6 a proposito mientras ese modal siga abierto (mismo
        // criterio que el clasico: ningun otro modal de trabajo puede estar abierto detras).
        var sectorModal = page.Locator("#modalSectoresPuntoExpendio");
        if (await sectorModal.IsVisibleAsync())
        {
            await sectorModal.Locator("button, a").First.ClickAsync();
            await page.WaitForTimeoutAsync(500);
        }

        await page.Keyboard.PressAsync("F6");
        await page.WaitForTimeoutAsync(1500);

        Assert.Equal(1, await page.Locator("#modalMisExpendiosPuntoExpendio.show").CountAsync());
        Assert.True(await page.Locator("#tablaMisExpendiosPuntoExpendio tbody tr").CountAsync() > 0);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Buscador avanzado de producto (F10), 2026-09-06 (retomado -- ver docs/DECISIONS.md). El
    // backend (ProductosController.ListarProductos) y el wiring de pos-product.js ya estaban
    // portados; faltaba solo modal-productos.js y el partial _BuscarProductoModal en las 2 vistas
    // de POS. Verifica el flujo completo: F10 abre el modal, la tabla carga productos reales
    // (no vacia), y el doble click cierra el modal y completa #inputCodigo con el codigo elegido.
    [Theory]
    [InlineData("/Ventas/POS")]
    [InlineData("/PuntosExpendio/POS")]
    public async Task POS_F10_AbreBuscadorAvanzadoYSeleccionaProducto(string ruta)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}{ruta}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        var sectorModal = page.Locator("#modalSectoresPuntoExpendio");
        if (await sectorModal.IsVisibleAsync())
        {
            await sectorModal.Locator("button, a").First.ClickAsync();
            await page.WaitForTimeoutAsync(500);
        }

        await page.Keyboard.PressAsync("F10");
        await page.WaitForTimeoutAsync(800);

        Assert.Equal(1, await page.Locator("#modalBuscarProducto.show").CountAsync());
        Assert.True(await page.Locator("#modalBuscarProducto tbody tr").CountAsync() > 0);

        await page.Locator("#modalBuscarProducto tbody tr").First.DblClickAsync();
        await page.WaitForTimeoutAsync(800);

        Assert.Equal(0, await page.Locator("#modalBuscarProducto.show").CountAsync());
        Assert.False(string.IsNullOrWhiteSpace(await page.Locator("#inputCodigo").InputValueAsync()));
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Historial de precios de cliente (F8), 2026-09-06 (retomado -- ver docs/DECISIONS.md). El
    // boton/atajo arranca oculto con Consumidor Final (gate de actualizarAccesoHistorialPreciosCliente)
    // y se habilita al elegir un cliente real via F9 -- verifica el gate de UX y que F8 abra el
    // modal generico con la clase compacta correcta (VentasController.HistorialPreciosCliente).
    [Fact]
    public async Task VentasPOS_F8_HabilitaConClienteRealYAbreHistorialPrecios()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);

        Assert.False(await page.Locator("#btnHistorialPreciosCliente").IsVisibleAsync());

        await page.Keyboard.PressAsync("F9");
        await page.WaitForTimeoutAsync(600);
        await page.Locator("#tablaPersonas tr.fila-persona").First.DblClickAsync();
        await page.WaitForTimeoutAsync(800);

        Assert.True(await page.Locator("#btnHistorialPreciosCliente").IsVisibleAsync());

        await page.Keyboard.PressAsync("F8");
        await page.WaitForTimeoutAsync(1200);

        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.show").CountAsync());
        Assert.Equal(1, await page.Locator("#modalFinanzasPOS.modal-historial-precios-pos-compacto").CountAsync());
        var contenido = await page.Locator("#contenedorFinanzasPOS").InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(contenido));
        Assert.DoesNotContain("Cargando", contenido);
        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Atajo "/" en cascada dentro del modal "Linea de venta" (2026-09-06, pedido explicito del
    // usuario -- ver docs/DECISIONS.md, para agilizar la bonificacion sin soltar el teclado).
    // Compartido entre Ventas/POS y PuntosExpendio/POS -- ambos reusan el mismo #modalLineaVenta
    // y pos-cart.js, un solo fix cubre las 2 pantallas. Paso 1 (bloque cerrado, foco fuera de un
    // input): equivalente a "B", abre el bloque de bonificar. Paso 2 (foco en el precio
    // bonificado): tilda "Bonificar por porcentaje". Paso 3 (foco en el porcentaje): tilda
    // "Aplicar a todos los productos". Cada paso debe consumir el "/" (no debe quedar tipeado en
    // el input).
    [Theory]
    [InlineData("/Ventas/POS")]
    [InlineData("/PuntosExpendio/POS")]
    public async Task POS_AtajoBarra_CascadaDeBonificacionSinSoltarElTeclado(string ruta)
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

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

        await page.ClickAsync("#tablaItems tr.fila-item");
        await page.WaitForTimeoutAsync(500);
        Assert.Equal(1, await page.Locator("#modalLineaVenta.show").CountAsync());
        Assert.Equal(0, await page.Locator("#bloqueBonificar:visible").CountAsync());

        // Guia de atajos al pie del modal (2026-09-06, pedido explicito del usuario -- ver
        // docs/DECISIONS.md): el texto va cambiando en cada paso para encaminar al usuario.
        Assert.Contains("Atajos", await page.Locator("#posLineaAtajosHint").InnerTextAsync());

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        Assert.Equal(1, await page.Locator("#bloqueBonificar:visible").CountAsync());
        Assert.False(await page.Locator("#chkPorcentaje").IsCheckedAsync());
        Assert.Contains("bonificar por porcentaje", await page.Locator("#posLineaAtajosHint").InnerTextAsync());

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        Assert.True(await page.Locator("#chkPorcentaje").IsCheckedAsync());
        Assert.False(await page.Locator("#chkBonificarTodos").IsCheckedAsync());
        Assert.DoesNotContain("/", await page.Locator("#txtPrecioKg").InputValueAsync());
        Assert.Contains("todos los ítems", await page.Locator("#posLineaAtajosHint").InnerTextAsync());

        await page.Keyboard.PressAsync("/");
        await page.WaitForTimeoutAsync(300);
        Assert.True(await page.Locator("#chkBonificarTodos").IsCheckedAsync());
        Assert.DoesNotContain("/", await page.Locator("#txtPorcentaje").InputValueAsync());
        Assert.Contains("Enter", await page.Locator("#posLineaAtajosHint").InnerTextAsync());

        Assert.Empty(errors);

        await page.CloseAsync();
    }

    // Atajo "5" (QR) del modal de forma de pago (item 4 de los 12 pendientes, 2026-09-09, ver
    // docs/DECISIONS.md "Batch C: POS forma de pago"). Causa raiz: Scripts/app/pos-forma-pago-
    // precios.js nunca se habia portado a WebCore -- sin el, normalizarTipoFormaPago("QR") (el
    // data-tipo del boton, en mayusculas) no coincidia con "Qr" (el valor del mapa de atajos), asi
    // que getBotonFormaPago() no encontraba el boton y el atajo no hacia nada. Los atajos 1-4 y 6
    // no se veian afectados porque sus data-tipo ya coinciden exactamente con el mapa sin
    // normalizar. Intercepta el POST real a FinalizarVenta para confirmar que efectivamente se
    // dispara con "Qr" sin crear una venta real en la base compartida de desarrollo.
    [Fact]
    public async Task VentasPOS_AtajoCinco_SeleccionaFormaDePagoQr()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        string? payloadCapturado = null;
        await page.RouteAsync("**/Ventas/FinalizarVenta", async route =>
        {
            payloadCapturado = route.Request.PostData;
            await route.FulfillAsync(new RouteFulfillOptions
            {
                Status = 200,
                ContentType = "application/json",
                Body = "{\"ok\":false,\"msg\":\"interceptado por test, no se guardo nada\"}"
            });
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

        // Foco fuera de cualquier input (el guard de 2026-09-07 desactiva los atajos numericos
        // mientras se esta escribiendo en el campo de % de descuento) -- estado normal al abrir
        // el modal recien, sin tocar el bloque de descuento.
        await page.ClickAsync("#modalFormaPago .modal-header", new PageClickOptions { Position = new Position { X = 5, Y = 5 } });

        await page.Keyboard.PressAsync("5");
        await page.WaitForTimeoutAsync(800);

        Assert.NotNull(payloadCapturado);
        Assert.Contains("\"formaPago\":\"Qr\"", payloadCapturado);
        Assert.Empty(errors);

        await page.CloseAsync();
    }
}
