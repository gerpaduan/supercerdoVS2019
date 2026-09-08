using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bugs reales reportados por el usuario (2026-09-07, ver docs/DECISIONS.md) en el modal "Mis
// actividades" del POS (F6):
//   7) la tabla de items se mostraba y despues se repetian los mismos items sin estructura --
//      causa: _LayoutPOS.cshtml nunca cargaba custom.css (el layout normal de la app si lo hace),
//      asi que la regla que oculta la vista de tarjetas movil cuando se ve la tabla de escritorio
//      nunca se aplicaba -- ambas estructuras quedaban visibles apiladas para los mismos datos.
//   8) el boton "Mis ventas" no hacia nada -- llamaba a window.POSVentas.abrirMis(), que nunca
//      se definia en esta vista (solo existia el equivalente admin, window.CajasAbiertas).
//   9) el subtitulo "ger | San Martin" (texto plano concatenado) se veia feo -- se separo en
//      vendedor (icono+nombre) y sucursal (badge).
[Collection("WebCore browser")]
public sealed class PosMisActividadesTests
{
    private readonly WebCoreFixture _fixture;

    public PosMisActividadesTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<IPage> AbrirPOSYActividadesAsync(WebCoreFixture fixture)
    {
        var page = await fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/POS", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.WaitForTimeoutAsync(500);
        await page.Locator("body").ClickAsync();
        await page.Keyboard.PressAsync("F6");
        await page.WaitForSelectorAsync("#modalFinanzasPOS.show", new PageWaitForSelectorOptions { Timeout = 10000 });
        await page.WaitForTimeoutAsync(600);
        return page;
    }

    [Fact]
    public async Task MisActividades_NoDuplicaItemsEnTarjetasMoviles()
    {
        var page = await AbrirPOSYActividadesAsync(_fixture);

        var tabla = page.Locator("#contenedorFinanzasPOS .egresos-tabla-desktop");
        var tarjetas = page.Locator("#contenedorFinanzasPOS .egresos-cards-mobile");
        Assert.True(await tabla.CountAsync() > 0, "no se encontro la tabla de escritorio");
        Assert.True(await tarjetas.CountAsync() > 0, "no se encontro el contenedor de tarjetas moviles");

        // En viewport de escritorio (default de la fixture), la tabla debe verse y las tarjetas
        // (que repiten los mismos items sin estructura de tabla) deben quedar ocultas.
        Assert.True(await tabla.IsVisibleAsync());
        Assert.False(await tarjetas.IsVisibleAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task MisActividades_BotonMisVentas_CargaLasVentasDelVendedor()
    {
        var page = await AbrirPOSYActividadesAsync(_fixture);

        var boton = page.Locator("#btnAbrirMisVentasDesdeActividades");
        Assert.True(await boton.CountAsync() > 0);
        await boton.ClickAsync();
        await page.WaitForTimeoutAsync(800);

        // El contenido del modal debe reemplazarse por la vista "Mis ventas" (mis-ventas-modal),
        // no quedar en blanco ni sin cambios.
        var misVentas = page.Locator("#contenedorFinanzasPOS .mis-ventas-modal");
        Assert.True(await misVentas.CountAsync() > 0, "el boton 'Mis ventas' no cargo nada -- ¿volvio a quedar sin window.POSVentas?");

        // El boton "Volver" de Mis ventas debe poder volver a Mis actividades.
        await page.Locator("#btnVolverMisActividadesDesdeVentas").ClickAsync();
        await page.WaitForTimeoutAsync(600);
        Assert.True(await page.Locator("#contenedorFinanzasPOS .mis-actividades-modal").CountAsync() > 0);

        await page.CloseAsync();
    }

    [Fact]
    public async Task MisActividades_SubtituloSeparaVendedorYSucursal()
    {
        var page = await AbrirPOSYActividadesAsync(_fixture);

        var vendedor = page.Locator("#contenedorFinanzasPOS .mis-actividades-vendedor");
        var sucursal = page.Locator("#contenedorFinanzasPOS .mis-actividades-sucursal-badge");
        Assert.True(await vendedor.CountAsync() > 0, "no se encontro el elemento de vendedor separado");
        Assert.True(await sucursal.CountAsync() > 0, "no se encontro el badge de sucursal separado");

        var textoVendedor = (await vendedor.InnerTextAsync()).Trim();
        var textoSucursal = (await sucursal.InnerTextAsync()).Trim();
        Assert.DoesNotContain("|", textoVendedor);
        Assert.DoesNotContain("|", textoSucursal);
        Assert.NotEmpty(textoVendedor);
        Assert.NotEmpty(textoSucursal);

        await page.CloseAsync();
    }
}
