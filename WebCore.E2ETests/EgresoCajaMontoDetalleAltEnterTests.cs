using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Pedidos reales (2026-09-15, ver docs/DECISIONS.md), todos sobre el modal "Nuevo Egreso de
// Caja" (Cajas/_AddOrEditEgresoCaja.cshtml):
// 1. #egresoMonto: solo numerico valido, puntos de miles en pantalla, "." tecleado = coma
//    decimal -- mismo mecanismo ya usado en Productos/Index.cshtml (MoneyInputMask).
// 2. #egresoDetalle: colapsado por defecto (alta nueva, sin contenido), con un switch para
//    revelarlo -- mismo patron que "Observacion del comprobante" en _FacturaElectronica.cshtml.
// 3. Alt+Enter guarda el registro (form ya editable).
[Collection("WebCore browser")]
public sealed class EgresoCajaMontoDetalleAltEnterTests
{
    private readonly WebCoreFixture _fixture;

    public EgresoCajaMontoDetalleAltEnterTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirNuevoEgresoAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Cajas/EgresosCaja", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.ClickAsync("#btnNuevoEgresoCaja");
        await page.Locator("#modalEgresoCaja.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.Locator("#egresoMonto").WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(300);
        return page;
    }

    [Fact]
    public async Task Monto_FormateaConPuntosDeMilesYPuntoTecleadoEsComaDecimal()
    {
        var page = await AbrirNuevoEgresoAsync(_fixture);

        await page.ClickAsync("#egresoMonto");
        await page.Keyboard.TypeAsync("1500");
        await page.Keyboard.PressAsync(".");
        await page.Keyboard.TypeAsync("5");

        // El bug real: sin la mascara, esto quedaria tal cual se tipeo ("1500.5"), sin puntos de
        // miles y con "." como separador (formato ingles, no el es-AR esperado).
        Assert.Equal("1.500,5", await page.Locator("#egresoMonto").InputValueAsync());

        // Letras y otros simbolos no deberian poder tipearse -- el campo es 100% numerico.
        await page.Keyboard.TypeAsync("abc$");
        Assert.Equal("1.500,5", await page.Locator("#egresoMonto").InputValueAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task Detalle_ArrancaColapsadoYElSwitchLoRevela()
    {
        var page = await AbrirNuevoEgresoAsync(_fixture);

        // El bug real: sin el fix, el textarea de Detalle estaba siempre visible, aunque
        // estuviera vacio (alta nueva).
        Assert.False(await page.Locator("#egresoDetalleWrap").IsVisibleAsync(), "el area de Detalle deberia arrancar colapsada en un alta nueva, sin contenido");
        Assert.False(await page.Locator("#egresoUsarDetalle").IsCheckedAsync());

        await page.ClickAsync("#egresoUsarDetalle");
        Assert.True(await page.Locator("#egresoDetalleWrap").IsVisibleAsync(), "activar el switch deberia revelar el area de Detalle");
        Assert.False(await page.EvaluateAsync<bool>("() => document.getElementById('egresoDetalle').disabled"));

        await page.ClickAsync("#egresoUsarDetalle");
        Assert.False(await page.Locator("#egresoDetalleWrap").IsVisibleAsync());
        Assert.True(await page.EvaluateAsync<bool>("() => document.getElementById('egresoDetalle').disabled"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task AltEnter_GuardaElRegistro()
    {
        var page = await AbrirNuevoEgresoAsync(_fixture);

        // Se elige del propio <select> ya renderizado (excluye tipos reservados en alta nueva) en
        // vez de una fuente de datos aparte, para no asumir que coinciden los ids disponibles.
        var opciones = await page.Locator("#idTipoEgresoCaja option").AllAsync();
        Assert.True(opciones.Count > 1, "hace falta al menos un tipo de egreso real (no reservado) en la base de dev para este test");
        await page.SelectOptionAsync("#idTipoEgresoCaja", new SelectOptionValue { Index = 1 });
        await page.FillAsync("#egresoDescripcion", "E2E AltEnter " + DateTime.UtcNow.Ticks);
        await page.ClickAsync("#egresoMonto");
        await page.Keyboard.TypeAsync("50");

        await page.Locator("#egresoMonto").PressAsync("Alt+Enter");

        // El bug real: sin el atajo, esto no disparaba nada -- solo el click en "Guardar egreso"
        // guardaba. Con el fix, Alt+Enter dispara el mismo submit (mismo criterio de exito que
        // el resto del flujo: el modal se cierra y/o el listado se refresca).
        await page.Locator("#modalEgresoCaja.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 10000 });

        await page.CloseAsync();
    }

    // Pedido real (2026-10-05, ver docs/DECISIONS.md): el contador de billetes llena el campo propio de solo
    // lectura (#egresoConteoBilletes), NO el Detalle, y desde esta pantalla no abre el modal de impresion de la
    // calculadora. "Limpiar contador" lo deja vacio.
    [Fact]
    public async Task ContadorDeBilletes_LlenaElConteoPropio_SinTocarElDetalleNiAbrirElModalDeImpresion()
    {
        var page = await AbrirNuevoEgresoAsync(_fixture);

        Assert.True(await page.EvaluateAsync<bool>("() => document.getElementById('egresoConteoBilletes').readOnly"), "el conteo debe ser de solo lectura");

        await page.ClickAsync("#btnCalculadoraBilletesEgreso");
        await page.Locator("#modalCalculadoraBilletes.show").WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        await page.FillAsync(".js-calculadora-billetes-cantidad[data-denominacion='1000']", "3");
        await page.ClickAsync("#btnAceptarCalculadoraBilletes");
        await page.Locator("#modalCalculadoraBilletes.show").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 5000 });

        Assert.Contains("3 x $1.000", await page.Locator("#egresoConteoBilletes").InputValueAsync());
        Assert.Equal("", await page.Locator("#egresoDetalle").InputValueAsync());
        Assert.Equal(0, await page.Locator("#modalPostCalculadoraBilletes.show").CountAsync());

        await page.ClickAsync("#btnLimpiarConteoBilletesEgreso");
        Assert.Equal("", await page.Locator("#egresoConteoBilletes").InputValueAsync());

        await page.CloseAsync();
    }

    // Pedido real (2026-10-05, ver docs/DECISIONS.md): al guardar ya no hay un SweetAlert de 2 s (era del
    // 2026-09-15) sino el modal de comprobante (1 cerrar / 2 ticket / 3 PDF / 4 mail). No se cierra con Escape
    // (solo eligiendo una opcion) y la opcion 1 lo cierra.
    [Fact]
    public async Task Guardar_MuestraModalDeComprobanteQueSoloSeCierraEligiendoUnaOpcion()
    {
        var page = await AbrirNuevoEgresoAsync(_fixture);

        var opciones = await page.Locator("#idTipoEgresoCaja option").AllAsync();
        Assert.True(opciones.Count > 1, "hace falta al menos un tipo de egreso real (no reservado) en la base de dev para este test");
        await page.SelectOptionAsync("#idTipoEgresoCaja", new SelectOptionValue { Index = 1 });
        await page.FillAsync("#egresoDescripcion", "E2E SwalExito " + DateTime.UtcNow.Ticks);
        await page.ClickAsync("#egresoMonto");
        await page.Keyboard.TypeAsync("50");

        await page.ClickAsync("#btnGuardarEgresoCaja");

        var modal = page.Locator("#modalPostComprobante.show");
        await modal.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });
        Assert.Contains("Egreso guardado", await modal.Locator("#pcTitulo").InnerTextAsync());

        // Escape NO lo cierra (modal estatico): solo se sale eligiendo una opcion.
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForTimeoutAsync(400);
        Assert.True(await modal.IsVisibleAsync(), "Escape no deberia cerrar el modal de comprobante");

        await page.ClickAsync("#btnPcCerrar");
        await modal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 3000 });

        await page.CloseAsync();
    }
}
