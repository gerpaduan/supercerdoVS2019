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
}
