using Microsoft.Playwright;

namespace WebCore.E2ETests;

// Bug real reportado por el usuario (2026-09-07, ver docs/DECISIONS.md): el dashboard de
// Home/Index no mostraba ningun dato ($0,00 / 0 en todos los KPIs), pese a que los 9 endpoints
// AJAX devolvian 200 con datos reales (confirmado con curl en un turno anterior). Causa real,
// reproducida con Playwright: WebCore serializa JSON con System.Text.Json (camelCase por
// default), pero home-dashboard.js -- portado literal del clasico, que corria contra
// Newtonsoft.Json (PascalCase) -- leia las propiedades en PascalCase. En JS leer una propiedad
// inexistente da "undefined" sin tirar error, asi que el bug nunca se vio en la consola ni en
// curl (que solo mira el JSON crudo). Fix: home-dashboard.js pasa a leer camelCase, igual que ya
// lee reportes.js (el resto de los modulos migrados). Este test verifica el KPI mas simple
// (cantidadVentas) contra un valor real esperado no-cero.
[Collection("WebCore browser")]
public sealed class HomeDashboardTests
{
    private readonly WebCoreFixture _fixture;

    public HomeDashboardTests(WebCoreFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Dashboard_MuestraKpisReales_NoQuedaEnCero()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        // No se usa WaitUntilState.NetworkIdle: home-dashboard.js llama initBalanza(), que hace
        // poll continuo al agente local de balanza (CarnisysBalanza) -- la red nunca queda
        // "idle" en esta pagina, el Goto colgaria hasta el timeout.
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        // El dashboard carga los KPIs por AJAX en secuencia (cargarResumen, cargarVentasPorHora,
        // etc.) -- dar tiempo a que la cadena termine.
        await page.WaitForTimeoutAsync(2000);

        Assert.Empty(errors);

        var cantidadVentasTexto = await page.Locator("[data-kpi='cantidadVentas']").InnerTextAsync();
        Assert.True(int.TryParse(cantidadVentasTexto.Trim(), out var cantidadVentas), $"No se pudo parsear '{cantidadVentasTexto}' como numero");
        Assert.True(cantidadVentas > 0, "cantidadVentas quedo en 0 -- el bug de mapeo PascalCase/camelCase volvio, o no hay ventas en el seed de datos.");

        var ventasTotalesTexto = await page.Locator("[data-kpi='ventasTotales']").InnerTextAsync();
        Assert.NotEqual("$ 0,00", ventasTotalesTexto.Trim());

        // Tabla de "ultimas ventas" -- debe tener filas reales, no el estado vacio.
        var filasUltimasVentas = await page.Locator("#tablaUltimasVentas tr").CountAsync();
        Assert.True(filasUltimasVentas > 0);
        var primeraFila = await page.Locator("#tablaUltimasVentas tr").First.InnerTextAsync();
        Assert.DoesNotContain("No hay ventas", primeraFila);

        await page.CloseAsync();
    }
}
