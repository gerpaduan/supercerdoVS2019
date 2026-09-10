using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Tercera ronda de pedidos (2026-09-10, ver docs/DECISIONS.md): "con el user produccion, este
// mensaje aparece siempre aunque tenga la caja abierta. Esta notificacion solo debe aparecer a
// verificar que no tiene caja abierta en la sucursal que esta logueado."
//
// Causa raiz real (4 sitios en VentasController.cs, verificados linea por linea contra el
// clasico): la caja se abre a nombre del OPERADOR real resuelto por ResolverOperadorPOS
// (CajasController.AbrirCaja: UsuarioInicio=operador), pero la busqueda de "hay caja abierta" en
// POS()/ModificarVenta()/FinalizarVenta() usaba "user" (la cuenta compartida de produccion) en
// vez de ese mismo operador -- nunca encontraba la caja real, asi que la advertencia quedaba
// encendida siempre que la cuenta compartida tuviera el permiso Venta.UltimaVenta (o fuera
// Admin). Se corrigieron los 4 sitios para usar "operador" consistentemente. Un 4to sitio
// (FinalizarVenta) se encontro durante la implementacion, no en el reporte original -- confirmado
// que el clasico (Web/Controllers/VentasController.cs:596) ya usaba "operador" ahi, asi que era
// una divergencia real de WebCore, no una decision deliberada.
[Collection("WebCore browser")]
public sealed class AdvertenciaCajaCerradaProduccionTests
{
    private readonly WebCoreFixture _fixture;

    public AdvertenciaCajaCerradaProduccionTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task VentasPOS_OperadorConCajaRealAbierta_NoMuestraAdvertenciaDeCajaCerrada()
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.FillAsync("input[name='Usuario']", "produccion");
        await page.FillAsync("input[name='Clave']", "a");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        if (await page.Locator("#txtSeleccionUsuario").CountAsync() > 0)
        {
            await page.FillAsync("#txtSeleccionUsuario", "ger");
            await page.WaitForTimeoutAsync(300);
            await page.Locator(".seleccion-usuario-item").First.ClickAsync();
            await page.FillAsync("#passSeleccionUsuario", "a");
            await page.ClickAsync("#btnConfirmarSeleccionUsuario");
            await page.WaitForTimeoutAsync(1500);
        }

        // Si al operador "ger" todavia no le quedo una caja real abierta de una corrida
        // anterior, se abre una ahora -- el escenario del bug es especificamente "produccion +
        // operador CON caja real abierta a su nombre".
        if (await page.Locator("#modalAbrirCaja.show").CountAsync() > 0)
        {
            await page.FillAsync("#montoInicial", "1000");
            await page.EvaluateAsync("window.abrirCaja()");
            await page.WaitForTimeoutAsync(2000);
        }

        // Assert principal: antes del fix, esto fallaba (la advertencia aparecia igual aunque
        // "ger" tuviera la caja realmente abierta, porque se buscaba con la cuenta compartida).
        Assert.Equal(0, await page.Locator("#alertaCajaCerradaPOS:visible").CountAsync());
    }

    [Fact]
    public async Task VentasPOS_UsuarioNoProduccion_SigueFuncionandoIgualQueAntes()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(600);

        // Con un usuario normal (no produccion), ResolverOperadorPOS devuelve la misma persona
        // sin cambios -- el fix no debe alterar nada observable: no debe haber ningun error de
        // servidor ni quedar la pantalla rota.
        Assert.Equal(0, await page.Locator("#modalSeleccionUsuario.show").CountAsync());
        Assert.True(await page.Locator("#inputCodigo").CountAsync() > 0, "el formulario de venta deberia renderizar normalmente");
    }
}
