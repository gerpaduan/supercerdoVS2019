using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado 2026-09-12 (ver docs/DECISIONS.md): VentasController.Index() ("Historial de
// Ventas") no validaba ningun permiso -- cualquier usuario autenticado podia ver el historial
// completo de ventas aunque tuviera "Puede ver" desactivado para el formulario Ventas
// (Entidades.Permisos.Venta.VerVentas). "caja" y "a" son usuarios reales de la base de dev con
// DiasPermitidosVer=-1 para ese formulario (equivale a "Puede ver" desactivado -- la formula de
// Negocio.Usuario.tienePermiso exige fechaDesde >= mañana, imposible para una fecha real).
[Collection("WebCore browser")]
public sealed class VentasIndexPermisoTests
{
    private readonly WebCoreFixture _fixture;

    public VentasIndexPermisoTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<IPage> LoginComoAsync(string usuario, string clave)
    {
        var page = await _fixture.Browser.NewPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Login");
        await page.FillAsync("input[name='Usuario']", usuario);
        await page.FillAsync("input[name='Clave']", clave);
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(url => !url.Contains("/Login"), new PageWaitForURLOptions { Timeout = 15000 });
        return page;
    }

    [Fact]
    public async Task VentasIndex_UsuarioSinPermisoVerVentas_MuestraAccesoDenegado()
    {
        var page = await LoginComoAsync("caja", "a");

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        // El bug real: sin el fix, esto mostraba el historial completo de ventas (con su form de
        // filtros) en vez de la vista de Acceso Denegado.
        Assert.Equal(1, await page.Locator("text=Acceso Denegado").CountAsync());
        Assert.Equal(0, await page.Locator("#formFiltroFechas").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task VentasIndex_UsuarioAdmin_SigueViendoElHistorialNormalmente()
    {
        // Regresion: el gate nuevo no debe romper el acceso normal de un usuario con permiso
        // (Admin, mismo criterio ya usado en el resto de la suite).
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        Assert.Equal(0, await page.Locator("text=Acceso Denegado").CountAsync());
        Assert.Equal(1, await page.Locator("#formFiltroFechas").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task VentasFacturas_UsuarioSinPermisoVerVentas_MuestraAccesoDenegado()
    {
        // Mismo bug/mismo permiso que Index() -- Facturas() usa Permisos.Venta.VerVentas
        // (ver docs/DECISIONS.md, 2026-09-12).
        var page = await LoginComoAsync("caja", "a");

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/Facturas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        Assert.Equal(1, await page.Locator("text=Acceso Denegado").CountAsync());

        await page.CloseAsync();
    }

    [Fact]
    public async Task VentasFacturas_UsuarioAdmin_SigueViendoElListadoNormalmente()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Ventas/Facturas", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await page.WaitForTimeoutAsync(400);

        Assert.Equal(0, await page.Locator("text=Acceso Denegado").CountAsync());

        await page.CloseAsync();
    }
}
