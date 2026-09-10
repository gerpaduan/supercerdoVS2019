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

    // Bug real (2026-09-08, ver docs/DECISIONS.md): el <h1> del hero ("Resumen del negocio en una
    // sola pantalla") tiene la clase "font-weight-bold", y ui-refresh.css fuerza
    // "color: var(--ui-text) !important" para esa clase -- en modo claro --ui-text es oscuro,
    // texto oscuro sobre el fondo navy fijo del hero = casi ilegible. Fix puntual con el mismo
    // !important en Home/Index.cshtml.
    [Fact]
    public async Task DashboardHero_TituloLegibleEnModoClaro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var h1 = page.Locator(".dashboard-hero h1");
        Assert.True(await h1.CountAsync() > 0);
        var color = await h1.EvaluateAsync<string>("el => getComputedStyle(el).color");
        Assert.Equal("rgb(255, 255, 255)", color);

        await page.CloseAsync();
    }

    // Mismo bug que el del <h1> de arriba, pero en "Tablero diario"/"Periodo"/"Sucursal" (item 5
    // de la segunda ronda de pedidos, 2026-09-10, ver docs/DECISIONS.md) -- pisados por 2 reglas
    // genericas mas de ui-refresh.css (".font-weight-bold" y "label"). Fix scoped a
    // .dashboard-hero en vez de tocar las reglas genericas (para no repetir el mismo error en
    // otro lugar del sitio).
    [Fact]
    public async Task DashboardHero_TableroPeriodoSucursal_LegiblesEnModoClaro()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(500);

        var tablero = page.Locator(".dashboard-hero .text-uppercase.font-weight-bold").First;
        Assert.True(await tablero.CountAsync() > 0);
        Assert.Equal("rgb(255, 255, 255)", await tablero.EvaluateAsync<string>("el => getComputedStyle(el).color"));

        var periodo = page.Locator("label[for='dashboardPeriodo']");
        Assert.True(await periodo.CountAsync() > 0);
        Assert.Equal("rgba(255, 255, 255, 0.6)", await periodo.EvaluateAsync<string>("el => getComputedStyle(el).color"));

        var sucursal = page.Locator("label[for='dashboardSucursal']");
        Assert.True(await sucursal.CountAsync() > 0);
        Assert.Equal("rgba(255, 255, 255, 0.6)", await sucursal.EvaluateAsync<string>("el => getComputedStyle(el).color"));

        await page.CloseAsync();
    }

    // Item 4 de la segunda ronda de pedidos (2026-09-10, ver docs/DECISIONS.md "Batch 8:
    // Actividades en el dashboard"): "agregar las actividades al dashboard, que muestre las
    // ultimas 10 actividades... que el bloque actividades sea el primero, en el lugar que hoy
    // ocupa ventas por hora." Reusa WebCore/Services/ActividadesFeedService.cs (extraido de
    // ActividadesController.cs, refactor de extraccion pura sin cambio de comportamiento --
    // verificado que /Actividades sigue mostrando los mismos items). "Ventas por hora" se
    // reubica (no se elimina) a una fila propia debajo (decision confirmada con el usuario).
    [Fact]
    public async Task DashboardActividades_EsElPrimerBloqueConLinkYVentasPorHoraSigueExistiendo()
    {
        var page = await _fixture.NewAuthenticatedPageAsync();
        var errors = new List<string>();
        page.PageError += (_, msg) => errors.Add(msg);

        await page.GotoAsync($"{WebCoreFixture.BaseUrl}/Home/Index", new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 20000 });
        await page.WaitForTimeoutAsync(2500);

        // "Actividades" debe ser el primer card-header con h6 dentro de una .row (despues de la
        // fila de KPIs, que no tiene h6 de card-header) -- confirma la posicion pedida.
        var primerTitulo = await page.EvaluateAsync<string?>(@"() => {
            const rows = document.querySelectorAll('.dashboard-shell > .row');
            for (const row of rows) {
                const h6 = row.querySelector('.card-header h6');
                if (h6) return h6.textContent.trim();
            }
            return null;
        }");
        Assert.NotNull(primerTitulo);
        Assert.StartsWith("Actividades", primerTitulo);

        var lista = page.Locator("#dashboardActividadesLista");
        Assert.True(await lista.CountAsync() > 0);
        var contenido = await lista.InnerTextAsync();
        Assert.False(string.IsNullOrWhiteSpace(contenido), "El bloque de Actividades no debería quedar vacío en la base de desarrollo (tiene datos reales recientes).");
        Assert.DoesNotContain("Cargando...", contenido);

        // Link del titulo -> pantalla completa de Actividades.
        var link = page.Locator(".dashboard-shell a[href*='/Actividades']").First;
        Assert.True(await link.CountAsync() > 0);
        Assert.Equal("/Actividades", await link.GetAttributeAsync("href"));

        // "Ventas por hora" sigue existiendo, solo reubicado.
        Assert.True(await page.Locator("#dashboardVentasHoraChart").CountAsync() > 0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
